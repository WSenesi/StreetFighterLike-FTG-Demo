using System;
using Sirenix.OdinInspector;

public interface ISignal
{
    int Duration { get; }
}
[Flags]
public enum Direction
{
    Idle = 0,
    Front = 1,
    Back = 2,
    Down = 4,
    Up = 8,
    // Up = 8,
    // Down = 4,
    // Back = 2,
    // Front = 1,
    // Idle = 0,
    // FrontUp = 9,
    // FrontDown = 5,
    // BackUp = 10,
    // BackDown = 6,
}
[Serializable]
public class DirectionSignal : ISignal
{
    [HorizontalGroup("DirectionSignal")]
    [VerticalGroup("DirectionSignal/Direction")]
    public Direction direction = Direction.Idle;
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
        if (flag == Direction.Idle)
            return (direction & ~flag) == flag;
        return (direction & flag) == flag;
    }
}
