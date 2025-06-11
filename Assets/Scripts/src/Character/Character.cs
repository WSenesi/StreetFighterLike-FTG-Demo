using System;
using System.Collections.Generic;
using Mirror;
using src.Behavior_Layer;
using src.Behavior_Layer.EventConfig;
using src.Input_Layer;
using src.PresentationLayer;
using src.Request_Layer;
using UnityEngine;

namespace src.Character
{
    public class Character : NetworkBehaviour
    {
        // Component
        public Transform opponent;
        public AnimationController animationController;
        public CharacterMotor motor;

        // Config
        public int fullHealth = 10000;
        [Tooltip("角色招式输入配置，列表索引越小优先级越高")] public List<RequestSO> characterMoveRequests;
        [Tooltip("角色行为状态图配置")] public StateGraphSO stateGraphConfig;


        [NonSerialized] public InputLayer inputLayer;
        [NonSerialized] public RequestLayer requestLayer;
        [NonSerialized] public BehaviorLayer behaviorLayer;
        public ContextData context;
        public CharacterEventManager EventManager { get; private set; }
        
        private bool _isGameReady = false;
        private int _nextHitStunDuration;                       // 存储临时受击硬直，供状态读取
        private Vector2 _nextKnockbackVelocity;                 // 存储临时击退，供状态读取
        
        public static Action<Character> OnCharacterInitialized;

        #region UI

        // [HideInInspector]
        public ProgressBarPro healthBar;

        #endregion

        #region Life Time

        private void Awake()
        {
            animationController ??= GetComponent<AnimationController>();
            motor ??= GetComponent<CharacterMotor>();
            EventManager = new CharacterEventManager();
        }

        public override void OnStartClient()
        {
            if (opponentNetId != 0)
            {
                FindOpponentOnClient();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // 主机上的角色，在被添加到GameSessionManager后，其opponent引用会被直接设置
            // 但我们也需要一个时机来初始化它的逻辑层。
            // 我们可以依赖 AssignOpponents 之后的一个信号
        }

        private void Update()
        {
            if (!_isGameReady) return;
            
            // --- 阶段 1: 本地输入采集 ---
            if (isLocalPlayer)
            {
                inputLayer.Update();
                // TODO: 发送输入
                // ClientInputUpdate();
            }
            
            // --- 阶段 2: 主机权威逻辑
            if (isServer)
            {   // TODO: 主机驱动逻辑帧
                // --- 阶段 2.1: 行为层 - 碰撞预处理 ---
                // 从暂存区获取本帧要处理的碰撞，并进行初步的游戏规则判定（如攻击事件组命中唯一性）
                // 此方法会更新 _currentAttackStateRuntimeData
                behaviorLayer.ProcessCachedCollisions();
                behaviorLayer.PopulateContextData();
    
                // --- 阶段 2.2: 行为层 - 填充上下文数据 ---
                // 使用更新后的 _currentAttackStateRuntimeData 和其他角色状态来填充 _currentContextData
                // 确保 _currentContextData 反映了最新的碰撞结果和角色状态，供请求层使用
                context.isGrounded = motor.IsGrounded;
    
                // --- 阶段 2.3: 请求层 - 生成行为请求 ---
                // RequestLayer读取 _currentContextData (现在包含了最新的命中确认、帧数等信息)
                // 并结合 _inputLayer 的输入，生成行为请求 (包括可能的取消请求)
                requestLayer.Update();
    
                // --- 阶段 2.4: 行为层 - 执行状态机逻辑与动作 ---
                // BehaviorLayer 获取来自 RequestLayer 的最高优先级请求
                // CharacterActionRequest currentActionRequest = _requestLayer.GetHighestPriorityRequest();
                // BehaviorLayer驱动状态机处理请求、进行状态转换，并执行当前活动状态的OnLogic()
                // 活动状态的OnLogic()会使用_currentContextData来应用效果、触发新的表现层事件等
                behaviorLayer.ExecuteStateMachineLogic();
            }
            

            motor.SetFacingDirection(context.isFacingRight);
            // --- 阶段 6: 清理 (可选) ---
            // requestLayer.ClearProcessedRequests(); // 清理已处理的请求
            // _currentContextData.ClearPerFrameData(); // 清理ContextData中每帧更新的临时数据
        
        }

        private void OnDestroy()
        {
            behaviorLayer?.OnDestroy();
        }        

        #endregion

        /// <summary>
        /// 当所有依赖准备好后, 才执行最终的初始化
        /// </summary>
        public void InitializeForGameplay()
        {
            if (_isGameReady) return;
            if (opponent is null)
            {
                Debug.LogError($"Character {netId}: Attempted to initialize, but opponent is null!");
                return;
            }

            // 初始化 ContextData, 此时 opponent 有效
            context = new ContextData(transform, opponent, animationController, motor);
            netHealth = fullHealth;

            // 创建并启动逻辑层
            inputLayer = new InputLayer(context, isLocalPlayer);
            requestLayer = new RequestLayer(inputLayer, characterMoveRequests);
            behaviorLayer = new BehaviorLayer(this);
            
            inputLayer.Start();
            requestLayer.Start();
            behaviorLayer.Start();
            
            // 执行任何特定于服务器或客户端的额外初始化 (拓展点)
            if (isServer)
            {
                Debug.Log($"Server Character {netId} is now fully initialized.");
            }
            else
            {
                // TODO: 客户端发送 [Command] 通知服务器准备完毕
                Debug.Log($"Client Character {netId} is now fully initialized and ready for game.");
            }
            
            // 所有逻辑层创建后，触发事件
            Debug.Log($"Character {netId} has finished InitializeForGameplay. Firing OnCharacterInitialized event.");
            OnCharacterInitialized.Invoke(this);
            
            // TODO: 健壮性优化：通知 UI管理器 绑定该角色的引用
            
            // 标记准备就绪
            _isGameReady = true;
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

        #region Network
        
        [SyncVar(hook = nameof(OnNetPlayerIdChanged))] 
        public uint netPlayerId;

        [SyncVar(hook = nameof(OnOpponentIdSet))]
        public uint opponentNetId;
        
        [SyncVar(hook = nameof(OnNetHealthChanged))] 
        public int netHealth;

        [SyncVar(hook = nameof(OnNetIsFacingRightChanged))] 
        public bool netIsFacingRight;

        private void OnNetPlayerIdChanged(uint oldId, uint newId)
        {
            Debug.Log($"Player ID changed to {netPlayerId}");
        }

        private void OnOpponentIdSet(uint oldId, uint newId)
        {
            if (!isServer && newId != 0)
            {
                FindOpponentOnClient();
            }
            Debug.Log($"Player ID changed to {opponentNetId}");
        }
        private void OnNetHealthChanged(int oldHealth, int newHealth)
        {
            if (context is not null) context.health = newHealth;

            if (healthBar is not null)
            {
                healthBar.SetValue(newHealth, fullHealth);
            }
            Debug.Log($"Health changed to {netHealth}");
        }

        private void OnNetIsFacingRightChanged(bool oldFacing, bool newFacing)
        {
            this.context.isFacingRight = newFacing;
            Debug.Log($"FacingRight changed to {netIsFacingRight}");
        }

        [TargetRpc]
        public void TargetSetOpponent(NetworkConnection target, uint oppNetId)
        {
            this.opponentNetId = oppNetId;
            // 客户端在收到自己的对手Id后, 立刻在本地查找
            FindOpponentOnClient();
        }

        private void FindOpponentOnClient()
        {
            if (context.opponent is not null || isServer) return;
            
            if (NetworkClient.spawned.TryGetValue(opponentNetId, out var opponentIdentity))
            {
                opponent = opponentIdentity.transform;
                Debug.Log($"Client {netId}: Opponent {opponentNetId} FOUND!");

                InitializeForGameplay();
            }
            else
            {
                Debug.LogWarning($"Client {netId}: Could not find opponent {opponentNetId}");
            }
        }
        #endregion
    }
}

