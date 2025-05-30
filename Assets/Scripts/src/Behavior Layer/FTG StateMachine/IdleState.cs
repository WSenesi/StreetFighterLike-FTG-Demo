namespace src.Behavior_Layer.FTG_StateMachine
{
    public class IdleState<TStateID> : BehaviorState<TStateID>
    {
        private IdleConfigSO _behaviorData;

        public IdleState(IdleConfigSO behaviorData,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._behaviorData = behaviorData;
        }

        protected override void OnEnter(ContextData context)
        {
            if (_behaviorData is null) return;
            
            // 播放动画
            context.animationController?.PlayAnimation(_behaviorData.IdleAnimation);
        }

        protected override void OnLogic(ContextData contextData)
        {
            base.OnLogic(contextData);
            
        }

        protected override void OnExit(ContextData contextData)
        {
            base.OnExit(contextData);
        }
    }
}