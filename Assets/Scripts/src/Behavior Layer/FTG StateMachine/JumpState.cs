using UnityEngine;

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

        protected override void OnLogic(ContextData context)
        {
            _currentFrameInState++;
            
            if (_currentFrameInState == 4)
            {
                // 执行角色移动
                var xDir = GetXAxisDir(context.isFacingRight, _jumpDirection);
                context.motor.RequestJump( new Vector2(_horizontalVelocity * xDir, _verticalVelocity) );
            }
            else if (_currentFrameInState >= _duration)
            {
                FtgFSM.RequestStateChange(_defaultStateID); 
            }
        }

        protected override void OnExit(ContextData context)
        {
            base.OnExit(context);
        }
        
        private int GetXAxisDir(bool isFacingRight, JumpDirection jumpDirection)
        {
            switch (jumpDirection)
            {
                case JumpDirection.Forward:
                    return isFacingRight ? 1 : -1;
                case JumpDirection.Backward:
                    return isFacingRight ? -1 : 1;
                case JumpDirection.Neutral:
                default:
                    return 0;
            }
        }
    }
}