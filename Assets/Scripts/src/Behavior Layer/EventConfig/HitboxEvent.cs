using System;
using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    public enum AttackType
    {
        // TODO: 上中下段+投技    
    }

    [Serializable]
    public struct HitEffectData
    {
        public int damage;
        public int hitRecoveryFrame;
        public int blockRecoveryFrame;
        public AttackType type;
    }
    
    [Serializable]
    public class HitboxEvent : EventConfigBase
    {
        [Header("Hitbox Definition")] 
        public Vector3 position;
        [Header("Hitbox Definition")] 
        public Vector3 scale;

        public HitEffectData effectData;
        public Transform ownerTransform;
        
        public override void Execute(ContextData context)
        {
            
        }
    }
}