using UnityHFSM;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class FTGStateMachine<TStateID> : StateMachine<TStateID>
    {
        public ContextData ContextData { get; private set; }

        public override void Init()
        {
            base.Init();
            ContextData = default(ContextData);
        }
        
        
    }
}