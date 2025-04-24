using src.Behavior_Layer;
using src.Input_Layer;
using UnityEngine;

namespace src
{
    public class ContextData
    {
        public Transform owner;
        public Transform opponent;
        public CharacterContextFlag currentContextFlag;
        public MoveBehaviorSO currentStateID;
        
        // TODO: Define other runtime data
        public Direction dirInput;
        public Attack atkInput;
        public float distanceToOpponent;
        // public int healthPercent;
        // public int comboCount;
        
        // TODO: Component
        public AnimationController animationController;
        

        public ContextData(Transform owner, Transform opponent, AnimationController animationController,
            MoveBehaviorSO currentStateID = null,
            CharacterContextFlag currentContextFlag = CharacterContextFlag.None
            )
        {
            this.owner = owner;
            this.opponent = opponent;
            this.animationController = animationController;
            this.currentStateID = currentStateID;
            this.currentContextFlag = currentContextFlag;
            
            distanceToOpponent = Vector3.Distance(owner.transform.position, opponent.transform.position);
            
        }

        public bool ContainsFlag(CharacterContextFlag requiredFlags)
        {
            return (currentContextFlag & requiredFlags) == requiredFlags;
        }
    }
}