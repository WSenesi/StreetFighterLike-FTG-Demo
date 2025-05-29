namespace src.Behavior_Layer.FTG_StateMachine
{
    public class BlockStunState<TStateID> : BehaviorState<TStateID>
    {
        private readonly BlockStunConfigSO _behaviorData;
        private int _currentFrameInState;
        private readonly string _moveCompleteTrigger;
        
        public int CurrentFrameInState => _currentFrameInState;
        
        public BlockStunState(BlockStunConfigSO behaviorData, string moveCompleteTrigger,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            _behaviorData = behaviorData;
            _moveCompleteTrigger = moveCompleteTrigger;
        }

        protected override void OnEnter(ContextData contextData)
        {
            // 从 ContextData 获取硬直时间
            _currentFrameInState = FtgFSM.Character.GetAndConsumeNextHitStunDuration();
            
            // 播放动画
            contextData.animationController.PlayAnimation(_behaviorData.blockStunAnimation);
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