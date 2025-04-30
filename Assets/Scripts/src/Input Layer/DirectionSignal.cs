using System;
using Sirenix.OdinInspector;

namespace src.Input_Layer
{
    public interface ISignal
    {
        int Duration { get; }
    }
    [Flags]
    public enum Direction
    {
        None = 0,
        Front = 1,
        Back = 2,
        Down = 4,
        Up = 8,
    }
    [Serializable]
    public class DirectionSignal : ISignal
    {
        [HorizontalGroup("DirectionSignal")]
        [VerticalGroup("DirectionSignal/Direction")]
        public Direction direction = Direction.None;
        [VerticalGroup("DirectionSignal/Duration")]
        public int duration = 1;
    
        public int Duration => duration;
        public DirectionSignal(Direction direction, int duration)
        {
            this.direction = direction;
            this.duration = duration;
        }

        /// <summary>
        /// flag为非0数, 判断对应位是否为1; flag为0, 判断direction是否为全零
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool Contains(Direction flag)
        {
            if (flag == Direction.None)
                return (direction & ~flag) == flag;
            return (direction & flag) == flag;
        }
    }
}