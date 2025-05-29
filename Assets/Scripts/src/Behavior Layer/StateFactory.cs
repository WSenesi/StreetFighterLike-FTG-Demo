using System;
using src.Behavior_Layer.FTG_StateMachine;

namespace src.Behavior_Layer
{
    public enum StateType
    {
        Idle,
        Walk,
        Jump,
        Crouch,
        Attack,
        Buff,
        HitStun,
        BlockStun,
    }
    public class StateFactory
    {
        private readonly BaseBehaviorConfigSO _defaultStateID;
        private readonly string _moveCompleteTrigger;

        public StateFactory(BaseBehaviorConfigSO defaultStateID, string moveCompleteTrigger)
        {
            _defaultStateID = defaultStateID;
            _moveCompleteTrigger = moveCompleteTrigger;
        }
        
        public BehaviorState<BaseBehaviorConfigSO> Create(BaseBehaviorConfigSO behaviorData)
        {
            switch (behaviorData.BehaviorType)
            {
                case StateType.Idle:
                    return new IdleState<BaseBehaviorConfigSO>(behaviorData as IdleConfigSO, behaviorData.needsExitTime);
                case StateType.Walk:
                    return new MovementState<BaseBehaviorConfigSO>(behaviorData as MovementConfigSO, behaviorData.needsExitTime);
                case StateType.Jump:
                    return new JumpState<BaseBehaviorConfigSO>(behaviorData as JumpConfigSO, _defaultStateID, behaviorData.needsExitTime);
                case StateType.Crouch:
                    return new CrouchState<BaseBehaviorConfigSO>(behaviorData as CrouchConfigSO, behaviorData.needsExitTime);
                case StateType.Attack:
                    return new AttackState<BaseBehaviorConfigSO>(behaviorData as AttackConfigSO, _moveCompleteTrigger, behaviorData.needsExitTime);
                case StateType.Buff:
                    break;
                case StateType.HitStun:
                    return new HitStunState<BaseBehaviorConfigSO>(behaviorData as HitStunConfigSO, _moveCompleteTrigger, behaviorData.needsExitTime);
                case StateType.BlockStun:
                    return new BlockStunState<BaseBehaviorConfigSO>(behaviorData as BlockStunConfigSO, _moveCompleteTrigger, behaviorData.needsExitTime);
                default:
                    throw new Exception("在状态生成时发生错误：错误的行为类型");
            }
            return null;
        }
    }
}