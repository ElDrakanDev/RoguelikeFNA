using System;

namespace RoguelikeFNA
{
    [Flags]
    public enum CollisionLayer
    {
        None = 0,
        Entity = 1,
        Ground = 2,
        Interactable = 4,
        Projectile = 8,
    }
}
