using System;
using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public class AnimationEvent : EventConfigBase
    {
        public AnimationClip animationClip;
        public float transitionDuration;
        public override void Execute(ContextData context)
        {
            context.animationController.PlayAnimation(animationClip);
        }
    }
}