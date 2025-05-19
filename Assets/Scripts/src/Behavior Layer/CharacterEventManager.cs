using System;
using System.Collections.Generic;
using src.Behavior_Layer.EventConfig;
using src.PresentationLayer;

namespace src.Behavior_Layer
{
    /// <summary>
    /// 由 Character 实例化，生命周期与 Character 同步。
    /// 定义供行为层发布，表现层订阅的 System.Action 事件。
    /// 作为角色内部所有 C# 事件的中心枢纽
    /// </summary>
    public class CharacterEventManager
    {
        // 行为层发布，表现层监听
        public event Action<AnimationEventConfig, Character> OnAnimationTrigger;
        public event Action<SfxEventConfig, Character> OnSfxTrigger;
        public event Action<VfxEventConfig, Character> OnVfxTrigger;
        
        public event Action<HitboxEventConfig, Character> OnHitboxActivateRequest;
        public event Action<HitboxEventConfig, Character> OnHitboxDeactivateRequest;
        
        public event Action<HurtboxEventConfig, Character> OnHurtboxActivateRequest;
        public event Action<HurtboxEventConfig, Character> OnHurtboxDeactivateRequest;
        
        // 表现层发布 (如 Hitbox 检测碰撞), 行为层监听
        public event Action<List<DetectedCollisionInfo>, Character> OnRawCollisionsDetected;


        public void TriggerAnimationEvent(AnimationEventConfig config, Character character)
        {
            this.OnAnimationTrigger?.Invoke(config, character);
        }

        public void TriggerSfxEvent(SfxEventConfig config, Character character)
        {
            this.OnSfxTrigger?.Invoke(config, character);
        }

        public void TriggerVfxEvent(VfxEventConfig config, Character character)
        {
            this.OnVfxTrigger?.Invoke(config, character);
        }

        public void RequestActivateHitbox(HitboxEventConfig config, Character character)
        {
            this.OnHitboxActivateRequest?.Invoke(config, character);
        }

        public void RequestDeactivateHitbox(HitboxEventConfig config, Character character)
        {
            this.OnHitboxDeactivateRequest?.Invoke(config, character);
        }

        public void RequestActivateHurtbox(HurtboxEventConfig config, Character character)
        {
            this.OnHurtboxActivateRequest?.Invoke(config, character);
        }

        public void RequestDeactivateHurtbox(HurtboxEventConfig config, Character character)
        {
            this.OnHurtboxDeactivateRequest?.Invoke(config, character);
        }

        public void DetectedRawCollision(List<DetectedCollisionInfo> collisions, Character character)
        {
            this.OnRawCollisionsDetected?.Invoke(collisions, character);
        }
    }
}