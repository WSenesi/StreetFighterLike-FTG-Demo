using src.Behavior_Layer.FTG_StateMachine;

namespace src.Behavior_Layer
{
    public enum StateType
    {
        Idle,
        WalkForward,
        WalkBackward,
        JumpForward,
        JumpBackward,
        JumpNeutral,
        Attack,
        Buff,
        Stun,
    }
    public class StateFactory 
    {
        public AttackState<MoveBehaviorSO> Create(MoveBehaviorSO behaviorData, string moveCompleteTrigger)
        {
            switch (behaviorData.behaviorType)
            {
                case StateType.Attack:
                    return new AttackState<MoveBehaviorSO>(behaviorData, moveCompleteTrigger, behaviorData.needsExitTime);
            }
            return null;
        }
    }
}