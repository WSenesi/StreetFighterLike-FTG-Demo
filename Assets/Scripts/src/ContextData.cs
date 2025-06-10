using System;
using System.Collections.Generic;
using src.Behavior_Layer;
using src.Behavior_Layer.EventConfig;
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
        public int health;
        public int healthPercent;
        // public int comboCount;
        public bool isGrounded;
        public bool isFacingRight;
        public bool IsCancelWindowActive { get; set; }
        public bool IsHitConfirmedThisFrame { get; set; }
        public List<ProcessedHitResult> FinalizedHitsThisFrame { get; private set; }
        
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
            
            // distanceToOpponent = Vector3.Distance(owner.transform.position, opponent.transform.position);
            FinalizedHitsThisFrame = new List<ProcessedHitResult>();
        }

        public bool ContainsFlag(CharacterContextFlag requiredFlags)
        {
            return (currentContextFlag & requiredFlags) == requiredFlags;
        }

        public void ClearPerFrameData()
        {
            IsHitConfirmedThisFrame = false;
            FinalizedHitsThisFrame.Clear();
        }
    }
    
    public struct ProcessedHitResult
    {
        public Character.Character TargetCharacter;
        public HurtboxComponent TargetHurtbox;      // 具体的受击Hurtbox
        public HitboxEventConfig SourceHitboxConfig; // 造成命中的Hitbox配置
        public HitEffectData EffectToApply;
        public HitType HitType;
        // ... 其他效果标记 (如元素属性、特殊状态触发等)
    }

    public enum HitType
    {
        None,
        NormalHit,
        CounterHit,
        PunishCounter,
        Blocked,
    }
}