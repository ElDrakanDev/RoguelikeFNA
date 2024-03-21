using Nez;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Nez
{
    public static class DebugExt
    {
        [Conditional("DEBUG")]
        public static void DrawCircle(float x, float y, float radius, Color color)
        {
            if (!Core.DebugRenderEnabled)
                return;

            if (Core.Scene != null && Core.Scene.Camera != null)
                Graphics.Instance.Batcher.Begin(Core.Scene.Camera.TransformMatrix);
            else
                Graphics.Instance.Batcher.Begin();

            var rect = new Rectangle(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(radius), Mathf.RoundToInt(radius));
            Graphics.Instance.Batcher.Draw(Core.Content.LoadTexture(ContentPath.Textures.Debug_circle_png), rect);

            Graphics.Instance.Batcher.End();
        }
    }
}
