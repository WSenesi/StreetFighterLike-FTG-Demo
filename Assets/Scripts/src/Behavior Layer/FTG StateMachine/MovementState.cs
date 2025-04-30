using UnityEngine;

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
        }

        protected override void OnLogic(ContextData context)
        {
            // 更新位置
            var transform = context.owner;
            var xDir = GetXAxisDir(context.isFacingRight, _walkDirection);
            transform.position += _speed * Time.deltaTime * xDir;
        }

        protected override void OnExit(ContextData context)
        {
            base.OnExit(context);
        }

        private Vector3 GetXAxisDir(bool isFacingRight, WalkDirection walkDirection)
        {
            bool isForward = walkDirection == WalkDirection.Forward;
            bool isPositiveDirection = !(isFacingRight ^ isForward);
            return isPositiveDirection ? Vector3.right : Vector3.left;
        }
    }
}