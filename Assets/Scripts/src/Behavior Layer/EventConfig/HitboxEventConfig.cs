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
        public int recoveryFrame;
        public Vector2 knockbackForce;
        public bool isKnockDown;
    }
    
    [Serializable]
    public class HitboxEventConfig : EventConfigBase
    {
        public LayerMask targetLayer;           // Hurtbox 所在图层
        public Transform ownerTransform;
                
        [Header("Hitbox Definition")] 
        public Vector2 offset;
        public Vector2 size;
        public int attackInstanceGroupId;       // 用于标识逻辑上属于同一"攻击波次"或"攻击实例"的 Hitbox 组
        public AttackType type;                 // 攻击类型
        
        
        public HitEffectData onBlockEffect;        // 攻击数值配置
        public HitEffectData onHitEffect;
        public HitEffectData onCounterEffect;
        public HitEffectData onPunishCounterEffect;
        
        
        
    }
}