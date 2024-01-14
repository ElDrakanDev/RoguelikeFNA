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

            var tmxMap = CreateEntity("test-tilemap")
                .AddComponent(new TiledMapRenderer(
                    Content.LoadTiledMap(ContentPath.Tilemaps.Mosaic.Tiled.Mosaic_demo_tmx), "Walls_1", false)
                { RenderLayer = 1}
                );

            CreateEntity("demo-entity").SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(tmxMap));

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new FollowCamera(FindEntity("demo-entity")));
        }
    }
}
