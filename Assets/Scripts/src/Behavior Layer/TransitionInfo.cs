using System;

namespace src.Behavior_Layer
{
    [Serializable]
    public struct TransitionInfo<TStateID>
    {
        public TStateID from;
        public TStateID to;
        public string trigger;
    }
}