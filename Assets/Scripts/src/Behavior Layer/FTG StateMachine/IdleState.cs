using UnityEngine;
using UnityHFSM;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class IdleState<TStateID> : StateBase<TStateID>
    {
        private MoveBehaviorSO behaviorData;

        public IdleState(MoveBehaviorSO behaviorData,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this.behaviorData = behaviorData;
        }

        public override void OnEnter()
        {
            if (fsm is not FTGStateMachine<MoveBehaviorSO> FTGfsm)
            {
                Debug.LogError("StateMachine is null or not a FTGStateMachine.");
                return;
            }
            
            OnEnter(FTGfsm.ContextData);
        }

        private void OnEnter(ContextData context)
        {
            
        }
    }
}