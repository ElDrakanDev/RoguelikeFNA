using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class RelativeEntity : Component, IUpdatable
    {
        public Entity RelativeTo;
        public Vector2 Offset;
        public bool ApplyScale = true;

        public RelativeEntity(Entity relativeTo, Vector2 offset)
        {
            RelativeTo = relativeTo;
            Offset = offset;
        }

        public Vector2 Position => RelativeTo.Transform.Position + Offset;

        public void Update()
        {
            if (ApplyScale)
            {
                Transform.Position = RelativeTo.Transform.Position + Offset * RelativeTo.Transform.Scale;
                Transform.Scale = RelativeTo.Transform.Scale;
            }
            else
                Transform.Position = RelativeTo.Transform.Position + Offset;
        }
    }
}