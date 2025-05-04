namespace src.Behavior_Layer.FTG_StateMachine
{
    public class MovementState<TStateID> : BehaviorState<TStateID>
    {
        private readonly MovementConfigSO _behaviorData;
        private readonly WalkDirection _walkDirection;
        private readonly float _speed;
        
        public MovementState(MovementConfigSO behaviorData, bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            _behaviorData = behaviorData;
            _speed = _behaviorData.speed;
            _walkDirection = _behaviorData.walkDirection;
        }
        
        protected override void OnEnter(ContextData context)
        {
            if (_behaviorData is null) return;
            
            // 播放动画
            context.animationController?.PlayAnimation(_behaviorData.loopAnimation);
            
            var xDir = GetXAxisDir(context.isFacingRight, _walkDirection);
            context.motor.SetHorizontalMovement(xDir, _speed);
        }

        protected override void OnLogic(ContextData context)
        {
            // 更新位置
            // var xDir = GetXAxisDir(context.isFacingRight, _walkDirection);
            // var transform = context.owner;
            // transform.position += _speed * Time.deltaTime * xDir;
            // context.motor.SetHorizontalMovement(xDir, _speed);
        }

        protected override void OnExit(ContextData context)
        {
            context.motor.SetHorizontalMovement(0, _speed);
        }

        private int GetXAxisDir(bool isFacingRight, WalkDirection walkDirection)
        {
            bool isForward = walkDirection == WalkDirection.Forward;
            bool isPositiveDirection = !(isFacingRight ^ isForward);
            return isPositiveDirection ? 1 : -1;
        }
    }
}