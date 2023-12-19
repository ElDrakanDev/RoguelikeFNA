using System;

namespace RoguelikeFNA
{
    [Flags]
    public enum CollisionLayer
    {
        None = 0,
        Player = 1,
        Ground = 2,
        Enemy = 4,
    }
}
