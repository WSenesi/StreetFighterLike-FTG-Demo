using UnityHFSM;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class FTGStateMachine<TStateID> : StateMachine<TStateID> 
        where TStateID : BaseBehaviorConfigSO
    {
        public ContextData ContextData { get; set; }

        public FTGStateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
            : base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
        {
            
        }
        
        public override void Init()
        {
            base.Init();
        }

        public override void OnLogic()
        {
            ContextData.currentStateID = ActiveStateName;
            base.OnLogic();
        }
    }
}