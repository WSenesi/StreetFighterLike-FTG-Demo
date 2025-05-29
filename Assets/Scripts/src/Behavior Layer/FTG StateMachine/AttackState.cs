using System;
using src.Behavior_Layer.EventConfig;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class AttackState<TStateID> : BehaviorState<TStateID>
    {
        private readonly AttackConfigSO _behaviorData;
        private int _currentFrameInState;
        // private int _nextEventIndex;
        private readonly string _moveCompleteTrigger; 
        
        public int CurrentFrameInState => _currentFrameInState;
        public CharacterEventManager EventManager { get; private set; }

        public AttackState(AttackConfigSO behaviorData, string moveCompleteTrigger,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._behaviorData = behaviorData;
            this._moveCompleteTrigger = moveCompleteTrigger;
        }

        public override void Init()
        {
            EventManager = FtgFSM.EventManager;
        }

        protected override void OnEnter(ContextData context)
        {
            // 刷新攻击招式的运行时缓存
            FtgFSM.Character.behaviorLayer.ResetAttackStateRuntimeData();
            _currentFrameInState = 0;
            // _nextEventIndex = 0;
            
            ProcessEvents(context);
        }

        protected override void OnLogic(ContextData context)
        {
            _currentFrameInState++;
            
            ProcessEvents(context);

            if (_currentFrameInState >= _behaviorData.duration)
            {
                FtgFSM.Trigger(_moveCompleteTrigger); 
            }
        }
        
        /// <summary>
        /// 攻击被打断时应当清除当前执行中的判定事件
        /// </summary>
        /// <param name="contextData"></param>
        protected override void OnExit(ContextData contextData)
        {
            if (_behaviorData is not null && EventManager is not null)
            {
                foreach (var eventConfig in _behaviorData.events)
                {
                    switch (eventConfig)
                    {
                        case HitboxEventConfig hitboxEventConfig:
                            EventManager.RequestDeactivateHitbox(hitboxEventConfig, FtgFSM.Character);
                            break;
                        case HurtboxEventConfig hurtboxEventConfig:
                            EventManager.RequestDeactivateHurtbox(hurtboxEventConfig, FtgFSM.Character);
                            break;
                    }
                }
            }
            base.OnExit(contextData);
        }

        private void ProcessEvents(ContextData context)
        {
            if (_behaviorData is null) return;

            foreach (var eventConfig in _behaviorData.events)
            {
                if (_currentFrameInState == eventConfig.startFrame)
                {
                    // 激活事件
                    switch (eventConfig)
                    {
                        case AnimationEventConfig animationEventConfig:
                            EventManager.TriggerAnimationEvent(animationEventConfig, FtgFSM.Character);
                            break;
                        case HitboxEventConfig hitboxEventConfig:
                            EventManager.RequestActivateHitbox(hitboxEventConfig, FtgFSM.Character);
                            break;
                        case HurtboxEventConfig hurtboxEventConfig:
                            EventManager.RequestActivateHurtbox(hurtboxEventConfig, FtgFSM.Character);
                            break;
                        case SfxEventConfig sfxEventConfig:
                        case VfxEventConfig vfxEventConfig:
                        default:
                            throw new NotImplementedException($"Event type {eventConfig.GetType().FullName} not implemented");
                    }
                }
                else if (_currentFrameInState == eventConfig.startFrame + eventConfig.duration)
                {
                    // 停止事件
                    switch (eventConfig)
                    {
                        case AnimationEventConfig animationEventConfig:
                            EventManager.TriggerAnimationEvent(animationEventConfig, FtgFSM.Character);
                            break;
                        case HitboxEventConfig hitboxEventConfig:
                            EventManager.RequestDeactivateHitbox(hitboxEventConfig, FtgFSM.Character);
                            break;
                        case HurtboxEventConfig hurtboxEventConfig:
                            EventManager.RequestDeactivateHurtbox(hurtboxEventConfig, FtgFSM.Character);
                            break;
                        case SfxEventConfig sfxEventConfig:
                        case VfxEventConfig vfxEventConfig:
                        default:
                            throw new NotImplementedException($"Event type {eventConfig.GetType().FullName} not implemented");
                    }
                }
            }
        }
    }
}