using System;
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
        public static float AngleUnsigned(this Vector2 v1, Vector2 v2) => Math.Abs(Nez.Vector2Ext.Angle(v1, v2));
        public static Vector2 RotateX(this Vector2 v, float angle) => new Vector2(v.X * Mathf.Cos(angle) - v.Y * Mathf.Sin(angle), v.Y);
    }
}
