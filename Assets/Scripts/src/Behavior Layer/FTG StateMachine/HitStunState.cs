namespace src.Behavior_Layer.FTG_StateMachine
{
    public class HitStunState<TStateID> : BehaviorState<TStateID>
    {
        private readonly HitStunConfigSO _behaviorData;
        private int _currentFrameInState;
        private readonly string _moveCompleteTrigger;
        
        public int CurrentFrameInState => _currentFrameInState;
        
        public HitStunState(HitStunConfigSO behaviorData, string moveCompleteTrigger,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            _behaviorData = behaviorData;
            _moveCompleteTrigger = moveCompleteTrigger;
        }

        protected override void OnEnter(ContextData contextData)
        {
            // 获取硬直时间
            _currentFrameInState = FtgFSM.Character.GetAndConsumeNextHitStunDuration();
            // 播放动画
            contextData.animationController.PlayAnimation(_behaviorData.hitStunAnimation);
        }

        protected override void OnLogic(ContextData contextData)
        {
            _currentFrameInState--;

            if (_currentFrameInState <= 0)
            {
                FtgFSM.Trigger(_moveCompleteTrigger);
            }
        }
    }
}