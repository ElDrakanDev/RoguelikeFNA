using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoguelikeFNA
{
    public static class ComponentExt
    {
        public static T Closest<T>(this IList<T> list, Vector2 position) where T : Component
        {
            T closest = null;
            float closestDistance = float.MaxValue;

            foreach (var component in list)
            {
                float distance = Vector2.Distance(position, component.Transform.Position);
                if (distance < closestDistance)
                {
                    closest = component;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public static bool LineOfSight<T>(this T component, Vector2 from) where T : Component => component.Entity.LineOfSight(from);

        public static bool InRange(this Component component, Vector2 from, float range) => component.Entity.InRange(from, range);
    }
}
