using System;
using src.PresentationLayer;
using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public class HurtboxEventConfig : EventConfigBase
    {
        // public GameObject hurtboxObject;    // 使用直接获取对象引用的方式
        public HurtboxComponent hurtboxComponent;   // 获取对象物体上的组件
        
        public Vector2 colliderOffset;      // Hurtbox碰撞体相对于其GameObject的2D偏移
        public Vector2 colliderSize;        // Hurtbox碰撞体的2D尺寸
        public bool isActive = true;        // 通常Hurtbox事件是激活，但也可配置为临时禁用某个Hurtbox
        // public HurtboxType type;         // Hurtbox类型 (可选, 如Head, Body, Leg，用于精确打击判定)
        
    }
}