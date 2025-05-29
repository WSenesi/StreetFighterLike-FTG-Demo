using System;
using System.Collections.Generic;
using src.Behavior_Layer.FTG_StateMachine;
using src.PresentationLayer;
using src.Request_Layer;
using UnityEngine;

namespace src.Behavior_Layer
{
    [Serializable]
    public class BehaviorLayer
    {
        public Character Character { get; }
        private StateGraphSO _stateGraphConfig;
    
        // TODO: Other Component
        private CharacterEventManager _eventManager;
        private AnimationController _animationController;
        private CharacterMotor _characterMotor;
        
    
        private ReqPriorityQueue<RequestSO> _generatedRequests;
        private FTGStateMachine<BaseBehaviorConfigSO> _fsm;
        private ContextData _contextData;
        private CharacterContextFlag _currentContextFlag;
        private List<DetectedCollisionInfo> _pendingFrameCollisions = new();
        [SerializeField] private AttackStateRuntimeData _currentAttackStateRuntimeData = new();
        private readonly object _collisionLock = new object(); 

        public BehaviorLayer(Character character)
        {
            Character = character;
            _generatedRequests = character.requestLayer.generatedRequests;
            _eventManager = character.EventManager;
            _stateGraphConfig = character.stateGraphConfig;
            _contextData = character.context;
        
            _animationController = _contextData.animationController;
            _characterMotor = _contextData.motor;
        }
    
        public void Start()
        {
            // State 的 Init 方法在状态机 AddState 方法中调用，因此状态订阅事件的逻辑应该在构建状态机之前完成
            _fsm = StateGraphSO.BuildStateMachine(Character, _stateGraphConfig);
            _fsm.ContextData = _contextData;
            _fsm.Init();
            
            // 订阅事件管理器的 OnRawCollisionsDetected 
            if (_eventManager is not null)
            {
                _eventManager.OnRawCollisionsDetected += HandleRawCollisionsDetected;
            }
           
        }

        public void OnDestroy()
        {
            if (_eventManager is not null)
            {
                _eventManager.OnRawCollisionsDetected -= HandleRawCollisionsDetected;
            }
        }

        private void TryGetBehavior()
        {
            RequestSO request = null;
            MappingRuleSO mappingRule = null;
            if (_generatedRequests.TryDequeueIf(_contextData, CheckRequest, out request, out mappingRule))
            {
                var trigger = mappingRule.mappingResult;
                Debug.Log($"{mappingRule.name}: {trigger}");
                _fsm.Trigger(trigger);
            }
        }

        private bool CheckRequest(RequestSO request, ContextData context, out MappingRuleSO matchedRule)
        {
            // 遍历请求中所有 MappingRule
            foreach (var mappingRule in request.mappingRules)
            {
                // 检查当前mappingRule是否满足条件
                if (mappingRule.MatchesContext(context))
                {
                    matchedRule = mappingRule;
                    return true;
                }
            }
        
            matchedRule = null;
            return false;
        }

        /// <summary>
        /// 作为 CharacterEventManager.OnRawCollisionsDetected 事件的实际处理器（由Character.cs转发调用），
        /// 负责暂存原始碰撞数据到内部的 _pendingFrameCollisions。
        /// </summary>
        /// <param name="collisions">原始碰撞数据</param>
        /// <param name="attacker">攻击方角色</param>
        private void HandleRawCollisionsDetected(List<DetectedCollisionInfo> collisions, Character attacker)
        {
            // 验证攻击方
            if (attacker != Character) return;

            lock (_collisionLock)
            {
                // 初步过滤/暂存
                _pendingFrameCollisions.AddRange(collisions);
            }
        }
        
        /// <summary>
        /// 从内部的 _pendingFrameCollisions 获取数据，
        /// 应用“一次攻击事件组只对一个目标生效一次”规则，计算初步的命中结果，
        /// 计算结果更新至内部的 _currentAttackStateRuntimeData。
        /// 此方法会直接调用受击方的 ApplyHitEffects。
        /// </summary>
        public void ProcessCachedCollisions()
        {
            // 获取数据并清空 _pendingFrameCollisions
            List<DetectedCollisionInfo> collisionsToProcessThisFrame = GetAndClearPendingCollisions();
            
            // 清空 _currentAttackStateRuntimeData 的相关数据
            _currentAttackStateRuntimeData.FinalizedHitsThisFrame.Clear();
            _currentAttackStateRuntimeData.HitOccurredThisFrame = false;
            
            // 遍历 collisionsToProcess, 对每个原始碰撞数据, 检查是否对应Hitbox组已经命中目标
            // 如果未命中, 将目标计入对应组的 HashSet, 计算命中效果并应用数据
            if (collisionsToProcessThisFrame.Count <= 0) 
                return;
            foreach (var rawCollision in collisionsToProcessThisFrame)
            {
                // 获取 Hitbox 的组ID和目标，判定命中类型
                var hitboxGroupId = rawCollision.SrcHitboxConfig.attackInstanceGroupId;
                var target = rawCollision.TargetCharacter;
                var hitType = target.EvaluateHitType(rawCollision.SrcHitboxConfig, Character);
                var targetHurtbox = rawCollision.TargetHurtbox;
                
                if (hitType == HitType.None) { continue; }
                Debug.LogWarning($"{rawCollision.Attacker} hits {target} on {targetHurtbox.identifier}, type {hitType}");
                // 检查该Hitbox组是否已经命中该目标
                _currentAttackStateRuntimeData.TargetsHitByAttackEventGroupInThisMove.
                    TryGetValue(hitboxGroupId, out var hitTarget);
                    
                // 如果已经存储了组Id对应的 HashSet, 且 HashSet 记录了受击角色, 说明已经击中过了, 不再重复处理 
                if (hitTarget is not null && hitTarget.Contains(target.GetInstanceID()))
                {
                    continue;
                }

                // 将目标加入对应 Hitbox 组的 HashSet
                if (hitTarget is null)
                {
                    hitTarget = new HashSet<int>();
                    _currentAttackStateRuntimeData.TargetsHitByAttackEventGroupInThisMove.Add(hitboxGroupId, hitTarget);
                }
                hitTarget.Add(target.GetInstanceID());
                    
                _currentAttackStateRuntimeData.HitOccurredThisFrame = true;

                var hitResult = CalculateHitOutcome(rawCollision, hitType);
                target.ApplyHitEffects(hitResult, Character);
                _currentAttackStateRuntimeData.FinalizedHitsThisFrame.Add(hitResult);
            }
        }

        private List<DetectedCollisionInfo> GetAndClearPendingCollisions()
        {
            var collisionsToProcess = new List<DetectedCollisionInfo>();
            lock (_collisionLock)
            {
                if (_pendingFrameCollisions.Count > 0)
                {
                    collisionsToProcess.AddRange(_pendingFrameCollisions);
                    _pendingFrameCollisions.Clear();
                }
            }
            return collisionsToProcess;
        }

        /// <summary>
        /// 根据传入碰撞中的 HitboxEventConfig (包含攻击力、硬直、击退类型等基础攻击属性)和当前受击角色的状态(
        /// </summary>
        /// <param name="collisionInfo"></param>
        /// <returns></returns>
        private ProcessedHitResult CalculateHitOutcome(DetectedCollisionInfo collisionInfo, HitType hitType)
        {
            var srcHitboxConfig = collisionInfo.SrcHitboxConfig;
            var effectData = hitType switch
            {
                HitType.Blocked => srcHitboxConfig.onBlockEffect,
                HitType.NormalHit => srcHitboxConfig.onHitEffect,
                HitType.CounterHit => srcHitboxConfig.onCounterEffect,
                HitType.PunishCounter => srcHitboxConfig.onPunishCounterEffect,
                _ => throw new Exception($"Unknown HitType: {hitType}")
            };
            return new ProcessedHitResult()
            {
                TargetCharacter = collisionInfo.TargetCharacter,
                TargetHurtbox = collisionInfo.TargetHurtbox,
                SourceHitboxConfig = srcHitboxConfig,
                EffectToApply = effectData,
                HitType = hitType,
            };
        }
        
        /// <summary>
        /// 基于内部最新的碰撞处理结果、角色状态、当前输入等，填充传递给请求层和状态逻辑的ContextData实例。
        /// </summary>
        public void PopulateContextData()
        {
            _contextData.ClearPerFrameData();
            _contextData.IsHitConfirmedThisFrame = _currentAttackStateRuntimeData.HitOccurredThisFrame;
            _contextData.FinalizedHitsThisFrame.AddRange(_currentAttackStateRuntimeData.FinalizedHitsThisFrame);
        }

        /// <summary>
        /// 接收来自RequestLayer的最高优先级请求，
        /// 驱动内部的_stateMachine进行状态转换，
        /// 并调用当前活动状态的OnLogic(context)方法。
        /// </summary>
        public void ExecuteStateMachineLogic()
        {
            // 1. 尝试从请求队列获取能够执行的状态触发器
            TryGetBehavior();
            
            // 2. 执行状态
            _fsm.OnLogic();
        }

        public void ChangeToBlockStunState()
        {
            _fsm.Trigger(_stateGraphConfig.blockStunTrigger);
        }

        public void ChangeToHitStunState()
        {
            _fsm.Trigger(_stateGraphConfig.hitStunTrigger);
        }

        public void ResetAttackStateRuntimeData()
        {
            _currentAttackStateRuntimeData.ResetForNewAttackMove();
        }
    }
    
    [Serializable]
    public class AttackStateRuntimeData
    {
        // Key: attackEventGroupId, Value: Set of target character InstanceIDs hit by this group in this move
        public Dictionary<int, HashSet<int>> TargetsHitByAttackEventGroupInThisMove { get; private set; }
        public List<ProcessedHitResult> FinalizedHitsThisFrame { get; private set; } 
        public bool HitOccurredThisFrame { get; set; }

        public AttackStateRuntimeData()
        {
            TargetsHitByAttackEventGroupInThisMove = new Dictionary<int, HashSet<int>>();
            FinalizedHitsThisFrame = new List<ProcessedHitResult>(); // 初始化
        }

        public void ResetForNewAttackMove()
        {
            TargetsHitByAttackEventGroupInThisMove.Clear();
            FinalizedHitsThisFrame.Clear();
            HitOccurredThisFrame = false;
        }
    }
    
    
}
