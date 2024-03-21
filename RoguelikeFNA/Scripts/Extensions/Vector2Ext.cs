using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public static class Vector2Ext
    {
        public static Vector2 Normalized(this Vector2 v) => Vector2.Normalize(v);
        public static float GetDirectionAngle(this Vector2 v) => Mathf.Atan2(v.Y, v.X);
        public static Vector2 FromDirectionAngle(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        public static float Magnitude(this Vector2 v) => Mathf.Sqrt(v.X * v.X + v.Y * v.Y);
    }
}
