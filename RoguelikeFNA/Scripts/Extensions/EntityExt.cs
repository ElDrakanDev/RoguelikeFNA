using System.Collections.Generic;
using Nez;
using Microsoft.Xna.Framework;

namespace RoguelikeFNA
{
    public static class EntityExt
    {
        public static Entity Closest(this IList<Entity> list, Vector2 position)
        {
            Entity closest = null;
            float closestDistance = float.MaxValue;

            foreach(var entity in list)
            {
                float distance = Vector2.Distance(position, entity.Position);
                if(distance < closestDistance)
                {
                    closest = entity;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public static bool LineOfSight(this Entity entity, Vector2 from)
        {
            var ray = Physics.Linecast(from, entity.Position, (int)CollisionLayer.Ground);
            return ray.Collider is null;
        }

        public static bool InRange(this Entity entity, Vector2 from, float range)
        {
            float res = Vector2.Distance(from, entity.Position);
            return res <= range;
        }
    }
}
