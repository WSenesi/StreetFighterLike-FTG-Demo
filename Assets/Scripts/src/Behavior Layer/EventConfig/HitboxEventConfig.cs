using System;
using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    public enum AttackType
    {
        // 上中下段+投技    
        High,
        Low,
        Overhead,
        Throw,
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
    public class HitboxEventConfig : EventConfigBase
    {
        [Header("Hitbox Definition")] 
        public Vector2 offset;
        public Vector2 size;
        public int attackInstanceGroupId;       // 用于标识逻辑上属于同一"攻击波次"或"攻击实例"的 Hitbox 组
        public HitEffectData effectData;        // 攻击属性配置
        public LayerMask targetLayer;           // Hurtbox 所在图层
        public Transform ownerTransform;
        
    }
}