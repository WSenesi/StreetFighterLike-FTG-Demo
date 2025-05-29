using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using src;
using src.Behavior_Layer;
using src.Behavior_Layer.EventConfig;
using src.Input_Layer;
using src.PresentationLayer;
using src.Request_Layer;
using UnityEngine;

public class Character : SerializedMonoBehaviour
{
    // Component
    public Transform player;
    public Transform opponent;
    public AnimationController animationController;
    public CharacterMotor motor;
    public bool isLocalPlayer;
    public CharacterEventManager EventManager { get; private set; }

    // Config
    public int fullHealth = 10000;
    [Tooltip("角色招式输入配置，列表索引越小优先级越高")] public List<RequestSO> characterMoveRequests;
    [Tooltip("角色行为状态图配置")] public StateGraphSO stateGraphConfig;


    [NonSerialized] public InputLayer inputLayer;
    [NonSerialized, OdinSerialize] public RequestLayer requestLayer;
    [NonSerialized, OdinSerialize] public BehaviorLayer behaviorLayer;
    [NonSerialized, OdinSerialize] public ContextData context;

    private int _nextHitStunDuration;                       // 存储临时受击硬直，供状态读取
    private Vector2 _nextKnockbackVelocity;                 // 存储临时击退，供状态读取

    private void Awake()
    {
        // 获取组件引用
        animationController ??= GetComponent<AnimationController>();
        motor ??= GetComponent<CharacterMotor>();
        EventManager = new CharacterEventManager();

        // 初始化 ContextData
        context = new ContextData(player, opponent, animationController, motor);
        context.health = fullHealth;

        // 初始化
        inputLayer = new InputLayer(context, isLocalPlayer);
        requestLayer = new RequestLayer(inputLayer, characterMoveRequests);
        behaviorLayer = new BehaviorLayer(this);

    }

    private void Start()
    {
        inputLayer.Start();
        requestLayer.Start();
        behaviorLayer.Start();
    }

    private void Update()
    {
        // --- 阶段 1: 输入处理 ---
        inputLayer.Update();
        
        // --- 阶段 2: 行为层 - 碰撞预处理 ---
        // 从暂存区获取本帧要处理的碰撞，并进行初步的游戏规则判定（如攻击事件组命中唯一性）
        // 此方法会更新 _currentAttackStateRuntimeData
        behaviorLayer.ProcessCachedCollisions();
        behaviorLayer.PopulateContextData();

        // --- 阶段 3: 行为层 - 填充上下文数据 ---
        // 使用更新后的 _currentAttackStateRuntimeData 和其他角色状态来填充 _currentContextData
        // 确保 _currentContextData 反映了最新的碰撞结果和角色状态，供请求层使用
        context.isGrounded = motor.IsGrounded;

        // --- 阶段 4: 请求层 - 生成行为请求 ---
        // RequestLayer读取 _currentContextData (现在包含了最新的命中确认、帧数等信息)
        // 并结合 _inputLayer 的输入，生成行为请求 (包括可能的取消请求)
        requestLayer.Update();

        // --- 阶段 5: 行为层 - 执行状态机逻辑与动作 ---
        // BehaviorLayer 获取来自 RequestLayer 的最高优先级请求
        // CharacterActionRequest currentActionRequest = _requestLayer.GetHighestPriorityRequest();
        // BehaviorLayer驱动状态机处理请求、进行状态转换，并执行当前活动状态的OnLogic()
        // 活动状态的OnLogic()会使用_currentContextData来应用效果、触发新的表现层事件等
        behaviorLayer.ExecuteStateMachineLogic();

        motor.SetFacingDirection(context.isFacingRight);
        // --- 阶段 6: 清理 (可选) ---
        // requestLayer.ClearProcessedRequests(); // 清理已处理的请求
        // _currentContextData.ClearPerFrameData(); // 清理ContextData中每帧更新的临时数据
        
    }

    private void OnDestroy()
    {
        behaviorLayer.OnDestroy();
    }

    /// <summary>
    /// 被攻击方调用，应用最终判定的攻击效果。
    /// 此方法会改变角色状态
    /// </summary>
    /// <param name="hitResult"></param>
    /// <param name="attacker"></param>
    public void ApplyHitEffects(ProcessedHitResult hitResult, Character attacker)
    {
        var effect = hitResult.EffectToApply;
        Debug.LogWarning($"Get {hitResult.HitType} hit, Hit Effect: {effect.damage}/{effect.recoveryFrame}");
        switch (hitResult.HitType)
        {
            case HitType.Blocked:
                TakeDamage(effect.damage);
                _nextHitStunDuration = effect.recoveryFrame;
                _nextKnockbackVelocity = effect.knockbackForce;
                behaviorLayer.ChangeToBlockStunState();
                break;
            case HitType.NormalHit:
            case HitType.CounterHit:
                TakeDamage(effect.damage);
                _nextHitStunDuration = effect.recoveryFrame;
                _nextKnockbackVelocity = effect.knockbackForce;
                behaviorLayer.ChangeToHitStunState();
                break;
            
            case HitType.PunishCounter:
                TakeDamage(effect.damage);
                _nextHitStunDuration = effect.recoveryFrame;
                _nextKnockbackVelocity = effect.knockbackForce;
                behaviorLayer.ChangeToHitStunState();
                break;
            case HitType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    public int GetAndConsumeNextHitStunDuration()
    {
        var duration = _nextHitStunDuration;
        _nextHitStunDuration = 0;
        return duration;
    }

    public Vector2 GetAndConsumeNextKnockbackVelocity()
    {
        var velocity = _nextKnockbackVelocity;
        _nextKnockbackVelocity = Vector2.zero;
        return velocity;
    }
    
    /// <summary>
    /// 攻击方角色调用，评估命中交互，返回命中类型。
    /// 此方法不改变角色数据和状态，仅作判定。
    /// 此方法仅用于判断是否被格挡，特殊命中（Counter、Punish Counter等）由攻击方在BehaviorLayer.ProcessCachedCollisions()方法判断
    /// </summary>
    /// <param name="attacker">攻击角色</param>
    /// <param name="hitConfig">攻击效果</param>
    /// <returns></returns>
    public HitType EvaluateHitType(HitboxEventConfig hitConfig, Character attacker)
    {
        // 连招: 受击角色以及在受击硬直中, 被命中会刷新受击硬直时间
        if (context.currentStateID == stateGraphConfig.hitStun) return HitType.NormalHit;
        // 联防: 受击角色已经在防御硬直中, 被命中会刷新防御硬直时间
        if (context.currentStateID == stateGraphConfig.blockStun) return HitType.Blocked;
        // 其他情况: 如果受击角色不处于 Idle, Crouch, WalkBackward 状态, 则一定会受击
        
        
        if (context.currentStateID != stateGraphConfig.idle
            && context.currentStateID != stateGraphConfig.crouch
            && context.currentStateID != stateGraphConfig.walkBackward)
        {
            return HitType.NormalHit;
        }
            
        
        // 对受击角色的按键输入情况判定, 如果根据攻击类型正确输入方向则格挡成功, 否则失败
        var attackData = hitConfig;
        bool blockConditionsMet = false;
        switch (attackData.type)
        {
            case AttackType.High:
                blockConditionsMet = ContainsDirection(context.dirInput, Direction.Back);
                break;
            case AttackType.Low:
                blockConditionsMet = ContainsDirection(context.dirInput, Direction.Back | Direction.Down);
                break;
            case AttackType.Overhead:
                blockConditionsMet =  ContainsDirection(context.dirInput, Direction.Back) 
                           && !ContainsDirection(context.dirInput, Direction.Down);
                break;
            case AttackType.Throw:
                Debug.LogError("还未实现功能: 投技与拆投");
                blockConditionsMet = false;
                break;
            default:
                throw new ArgumentException("未知的攻击类型");
        }

        if (blockConditionsMet)
        {
            // 可能的拓展点：如果受击方操作上为格挡，特殊机制可以产生特殊效果
            return HitType.Blocked;
        }

        return HitType.NormalHit;
    }
    
    private bool ContainsDirection(Direction input, Direction required)
    {
        if (required == Direction.None)
            return (input & ~required) == required;
        return (input & required) == required;
    }

    private void TakeDamage(int damage)
    {
        context.health -= damage;
        // context.healthPercent = (float)context.health / (float)fullHealth;
    }
}

