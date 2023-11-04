using Microsoft.Xna.Framework;
using Nez;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    [System.Serializable]
    public class HitboxGroup
    {
        public List<RectangleF> Hitboxes;
        //[NotInspectable] public RectangleF Bounds { get; private set; }
        public RectangleF Bounds => GetBounds();

        public HitboxGroup()
        {
            Hitboxes = new List<RectangleF>();
        }
        public HitboxGroup(List<RectangleF> hitboxes)
        {
            Hitboxes = hitboxes;
        }

        RectangleF GetBounds()
        {
            Vector2 topLeftBounds = Vector2.One * float.MaxValue;
            Vector2 bottomRightBounds = -Vector2.One * float.MaxValue;

            foreach (var hitbox in Hitboxes)
            {
                float top = hitbox.Y;
                if (top < topLeftBounds.Y) topLeftBounds.Y = top;

                float bottom = hitbox.Y + hitbox.Height;
                if (bottom > bottomRightBounds.Y) bottomRightBounds.Y = bottom;

                float left = hitbox.X;
                if (left < topLeftBounds.X) topLeftBounds.X = left;

                float right = hitbox.X + hitbox.Width;
                if (right > bottomRightBounds.X) bottomRightBounds.X = right;
            }

            return new RectangleF(topLeftBounds, bottomRightBounds - topLeftBounds);
        }
    }


}
