using System;
using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public class AnimationEventConfig : EventConfigBase
    {
        public AnimationClip animationClip;
        public float transitionDuration;

    }
}