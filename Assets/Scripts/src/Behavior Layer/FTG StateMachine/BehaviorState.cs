using UnityEngine;
using UnityHFSM;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public abstract class BehaviorState<TStateID> : StateBase<TStateID>
    {
        protected FTGStateMachine<BaseBehaviorConfigSO> FtgFSM => fsm as FTGStateMachine<BaseBehaviorConfigSO>;

        protected BehaviorState(bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
        }

        protected virtual void OnEnter(ContextData contextData) {}
        protected virtual void OnLogic(ContextData contextData) {}
        protected virtual void OnExit(ContextData contextData) {}

        private bool EnsureFtgStateMachine()
        {
            if (FtgFSM is not null)
            {
                return true;
            }
            Debug.LogError("StateMachine is null or not a FTGStateMachine.");
            return false;
        }
        public override void OnEnter()
        {
            if (!EnsureFtgStateMachine())
                return;
            
            Debug.Log($"Entering state: {name}");
            OnEnter(FtgFSM.ContextData);
        }
        
        public override void OnLogic()
        {
            if (!EnsureFtgStateMachine())
                return;
            
            OnLogic(FtgFSM.ContextData);
        }

        public override void OnExit()
        {
            if (!EnsureFtgStateMachine())
                return;
            
            OnExit(FtgFSM.ContextData);
        }
    }
}