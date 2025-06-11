using System;

namespace src.Input_Layer
{
    [Serializable]
    public struct NetworkInputData
    {
        public uint tick;
        public Direction dirInput;
        public Attack atkInput;
    }
}
