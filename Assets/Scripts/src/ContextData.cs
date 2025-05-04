using System;
using src.Behavior_Layer;
using src.Input_Layer;
using src.PresentationLayer;
using UnityEngine;

namespace src
{
    [Serializable]
    public class ContextData
    {
        public Transform owner;
        public Transform opponent;
        
        // TODO: Define other runtime data
        public CharacterContextFlag currentContextFlag;
        public BaseBehaviorConfigSO currentStateID;
        public Direction dirInput;
        public Attack atkInput;
        public float distanceToOpponent;
        public int healthPercent;
        // public int comboCount;
        public bool isGrounded;
        public bool isFacingRight;
        
        // TODO: Component
        public AnimationController animationController;
        public CharacterMotor motor;

        public ContextData(Transform owner, Transform opponent, 
            AnimationController animationController,
            CharacterMotor motor,
            BaseBehaviorConfigSO currentStateID = null,
            CharacterContextFlag currentContextFlag = CharacterContextFlag.None
            )
        {
            this.owner = owner;
            this.opponent = opponent;
            this.animationController = animationController;
            this.motor = motor;
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