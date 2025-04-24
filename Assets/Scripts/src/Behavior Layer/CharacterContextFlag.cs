using System;

namespace src.Behavior_Layer
{
    [Flags]
    public enum CharacterContextFlag
    {
        None = 0,
        Ground = 1 << 0,
        Airborne = 1 << 1,
        Blocking = 1 << 2,
        // TODO: Define State & Flags
    }
}