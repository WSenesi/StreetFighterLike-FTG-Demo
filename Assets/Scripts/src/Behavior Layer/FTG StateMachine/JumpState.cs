namespace src.Behavior_Layer.FTG_StateMachine
{
    public class JumpState<TStateID> : BehaviorState<TStateID>
    {
        private readonly JumpConfigSO _behaviorData;
        private JumpDirection _jumpDirection;
        private float _verticalVelocity;
        private float _horizontalVelocity;
        private float _gravity;
        private readonly int _duration;
        private int _currentFrameInState;
        
        private readonly BaseBehaviorConfigSO _defaultStateID;
        
        public JumpState(JumpConfigSO behaviorData, BaseBehaviorConfigSO idle, bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._behaviorData = behaviorData;
            _jumpDirection = behaviorData.jumpDirection;
            _verticalVelocity = behaviorData.verticalVelocity;
            _horizontalVelocity = behaviorData.horizontalVelocity;
            _gravity = behaviorData.gravity;
            _duration = behaviorData.jumpDuration;
            _defaultStateID = idle;
        }
        
        protected override void OnEnter(ContextData context)
        {
            if (_behaviorData is null) return;
            
            _currentFrameInState = 0;
            
            // 播放动画
            context.animationController?.PlayAnimation(_behaviorData.jumpAnimation);
        }

        protected override void OnLogic(ContextData contextData)
        {
            _currentFrameInState++;
            
            if (_currentFrameInState >= _duration)
            {
                FtgFSM.RequestStateChange(_defaultStateID); 
            }
        }

        protected override void OnExit(ContextData contextData)
        {
            base.OnExit(contextData);
        }
    }
}