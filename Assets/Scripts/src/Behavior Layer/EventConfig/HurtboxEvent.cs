using System;
using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public class HurtboxEvent : EventConfigBase
    {
        public string TargetHurtboxId; // Hurtbox GameObject 的名字

        public bool SetEnabled = true; // true 表示启用, false 表示禁用
        public bool UseCustomOffset = false;
        public Vector2 CustomOffset;
        public bool UseCustomSize = false;
        public Vector2 CustomSize;
        
        public override void Execute(ContextData context)
        {
            throw new NotImplementedException();
        }
    }
}