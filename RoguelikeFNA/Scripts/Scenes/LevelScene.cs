using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    public class LevelScene : BaseScene
    {
        public override void Initialize()
        {
            base.Initialize();

            CreateEntity("test-tilemap")
                .SetScale(3)
                .AddComponent(new TiledMapRenderer(Content.LoadTiledMap(ContentPath.Tilemaps.Test.Tiled.AutoLayer_tmx), "IntGrid_layer"));

            CreateEntity("demo-entity").AddComponent(new DemoComponent());
        }
    }
}
