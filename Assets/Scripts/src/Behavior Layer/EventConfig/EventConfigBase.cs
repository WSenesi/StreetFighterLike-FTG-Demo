using System;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public abstract class EventConfigBase
    {
        public int startFrame;
        public int duration;

        public abstract void Execute(ContextData context);
    }
}

