using System.Collections.Generic;
using Nez;
using Microsoft.Xna.Framework;
using System.Linq;

namespace RoguelikeFNA
{
    public static class EntityExt
    {
        public static Entity Closest(this IEnumerable<Entity> list, Vector2 position)
        {
            Entity closest = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in list)
            {
                float distance = Vector2.Distance(position, entity.Position);
                if (distance < closestDistance)
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

        public static IEnumerable<Entity> Enabled(this IList<Entity> entities)
        {
            return entities.Where(e => e.Enabled);
        }

        public static void Destroy(this IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                entity.Destroy();
        }

        public static IEnumerable<Entity> Children(this Entity entity)
        {
            for (int i = 0; i < entity.Transform.ChildCount; i++)
            {
                yield return entity.Transform.GetChild(i).Entity;
            }
        }
    }
}
