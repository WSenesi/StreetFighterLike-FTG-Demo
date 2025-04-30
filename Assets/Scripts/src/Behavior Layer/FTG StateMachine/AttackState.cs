using UnityEngine;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class AttackState<TStateID> : BehaviorState<TStateID>
    {
        private readonly AttackConfigSO _behaviorData;
        private int _currentFrameInState;
        private int _nextEventIndex;
        private readonly string _moveCompleteTrigger; 
        
        public int CurrentFrameInState => _currentFrameInState;

        public AttackState(AttackConfigSO behaviorData, string moveCompleteTrigger,
            bool needsExitTime, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._behaviorData = behaviorData;
            this._moveCompleteTrigger = moveCompleteTrigger;
        }

        protected override void OnEnter(ContextData context)
        {
            _currentFrameInState = 0;
            _nextEventIndex = 0;

            ProcessEvents(context);
        }

        protected override void OnLogic(ContextData context)
        {
            _currentFrameInState++;
            
            ProcessEvents(context);

            if (_currentFrameInState >= _behaviorData.duration)
            {
                FtgFSM.Trigger(_moveCompleteTrigger); 
            }
        }
        
        private void ProcessEvents(ContextData context)
        {
            if (_behaviorData is null) return;
            
            var events = _behaviorData.events;
            while (_nextEventIndex < events.Count &&
                   events[_nextEventIndex].startFrame <= _currentFrameInState)
            {
                Debug.Log($"Execute Event:{events[_nextEventIndex].GetType().Name}");
                events[_nextEventIndex].Execute(context);
                _nextEventIndex++;
            }
        }
    }
}