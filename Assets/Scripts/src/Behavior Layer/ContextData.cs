using UnityEngine;

namespace src.Behavior_Layer
{
    public struct ContextData
    {
        public GameObject owner;
        public GameObject opponent;
        public CharacterContextFlag currentContextFlag;
        public MoveBehaviorSO currentStateID;
        
        // TODO: Define other runtime data
        
        public float distanceToOpponent;
        // public int healthPercent;
        // public int comboCount;
        
        // TODO: Component
        public AnimationController animationController;
        

        public ContextData(GameObject owner, GameObject opponent, AnimationController animationController,
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