using UnityHFSM;

namespace src.Behavior_Layer.FTG_StateMachine
{
    public class FTGStateMachine<TStateID> : StateMachine<TStateID> 
        where TStateID : BaseBehaviorConfigSO
    {
        public ContextData ContextData { get; set; }
        public Character Character { get; }
        public CharacterEventManager EventManager { get; }

        public FTGStateMachine(Character character, bool needsExitTime = false, 
            bool isGhostState = false, bool rememberLastState = false)
            : base(needsExitTime, isGhostState, rememberLastState)
        {
            this.Character = character;
            this.EventManager = character.EventManager;
        }

        public override void OnLogic()
        {
            ContextData.currentStateID = ActiveStateName;
            base.OnLogic();
        }
    }
}