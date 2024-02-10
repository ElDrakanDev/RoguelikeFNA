﻿using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class LevelScene : BaseScene
    {
        InputManager _inputManager;
        TiledMapRenderer _tmxMap;

        public override void Initialize()
        {
            base.Initialize();

            _tmxMap = CreateEntity("test-tilemap")
                .AddComponent(new TiledMapRenderer(
                    Content.LoadTiledMap(ContentPath.Tilemaps.Mosaic.Tiled._0001_Mosaic_demo_tmx), "Walls_1", true)
                { RenderLayer = 1}
                );

            _inputManager = Core.GetGlobalManager<InputManager>();
            foreach(var input in _inputManager.AvailablePlayers)
                AddPlayer(input);

            _inputManager.OnPlayerJoined += AddPlayer;

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new FollowCamera(FindComponentsOfType<DemoComponent>()[0].Entity));


            CreateEntity("test-enemy")
               .SetLocalPosition(new Vector2(300, 235))
               .AddComponent(new DemoEnemy());
               
        }

        void AddPlayer(PlayerInput input)
        {
            AddEntity(new Entity()).SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(_tmxMap, input));
        }
    }
}
