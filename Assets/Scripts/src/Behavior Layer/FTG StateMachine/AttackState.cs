using UnityEngine;
using UnityHFSM;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class AttackState<TStateID> : StateBase<TStateID>
    {
        private MoveBehaviorSO behaviorData;
        private int currentFrameInState;
        private int nextEventIndex;
        private readonly string MoveCompleteTrigger; 
        
        public int CurrentFrameInState => currentFrameInState;

        public AttackState(MoveBehaviorSO behaviorData, string moveCompleteTrigger,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this.behaviorData = behaviorData;
            this.MoveCompleteTrigger = moveCompleteTrigger;
        }

        public override void Init()
        {
            base.Init();
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
            currentFrameInState = 0;
            nextEventIndex = 0;

            ProcessEvents(context);
        }

        public override void OnLogic()
        {
            if (fsm is not FTGStateMachine<MoveBehaviorSO> FTGfsm)
            {
                Debug.LogError("StateMachine is null or not a FTGStateMachine.");
                return;
            }
            
            OnLogic(FTGfsm.ContextData);
        }

        private void OnLogic(ContextData context)
        {
            currentFrameInState++;
            
            ProcessEvents(context);

            if (currentFrameInState >= behaviorData.duration)
            {
                (fsm as StateMachine)?.Trigger(MoveCompleteTrigger); 
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
        
        private void ProcessEvents(ContextData context)
        {
            if (behaviorData is null) return;
            
            var events = behaviorData.events;
            while (nextEventIndex < events.Count &&
                   events[nextEventIndex].startFrame <= currentFrameInState)
            {
                events[nextEventIndex].Execute(context);
                nextEventIndex++;
            }
        }
    }
}