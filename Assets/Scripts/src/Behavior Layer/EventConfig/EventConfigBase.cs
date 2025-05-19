using System;

namespace src.Behavior_Layer.EventConfig
{
    [Serializable]
    public abstract class EventConfigBase
    {
        public string eventId = Guid.NewGuid().ToString();   // 事件ID，调试追踪
        public int startFrame;                                      // 事件在行为内的起始帧数
        public int duration;                                        // 事件持续帧数 (0或1表示尽在 startFrame 当帧有效)
        
    }
}

