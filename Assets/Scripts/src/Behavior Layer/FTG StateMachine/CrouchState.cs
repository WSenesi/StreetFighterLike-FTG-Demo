using UnityEngine;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class CrouchState<TStateID> : BehaviorState<TStateID>
    {
        private readonly CrouchConfigSO _behaviorData;
        
        public CrouchState(CrouchConfigSO behaviorData, bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._behaviorData = behaviorData;
        }
        
        protected override void OnEnter(ContextData context)
        {
            if (_behaviorData is null) return;
            
            // 播放动画
            context.animationController?.PlayAnimation(_behaviorData.crouchDownAnimation);
        }

        protected override void OnLogic(ContextData context)
        {
            base.OnLogic(context);
            
        }

        protected override void OnExit(ContextData context)
        {
            // 播放站起动画
            Debug.Log($"Exiting Crouch State: {_behaviorData.standUpAnimation.name}");
            context.animationController?.PlayAnimation(_behaviorData.standUpAnimation);
        }
    }
}