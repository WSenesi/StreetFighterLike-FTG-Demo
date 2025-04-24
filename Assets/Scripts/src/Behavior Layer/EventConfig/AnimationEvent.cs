using System;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public class AnimationEvent : EventConfigBase
    {
        public string animationName;
        public float transitionDuration;
        public override void Execute(ContextData context)
        {
            context.animationController.PlayAnimation(animationName, transitionDuration);
        }
    }
}