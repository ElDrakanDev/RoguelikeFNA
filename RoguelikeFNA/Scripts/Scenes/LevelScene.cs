﻿using Microsoft.Xna.Framework;
using Nez;
using RoguelikeFNA.Items;

namespace RoguelikeFNA
{
    public class LevelScene : BaseScene
    {
        InputManager _inputManager;
        public TiledMapRenderer activeTiledMap;

        public override void Begin()
        {
            base.Begin();

            activeTiledMap = CreateEntity("test-tilemap")
                .AddComponent(new TiledMapRenderer(
                    Content.LoadTiledMap(ContentPath.Tilemaps.Mosaic.Tiled._0001_Mosaic_demo_tmx), "Walls_1", true)
                { RenderLayer = 1, PhysicsLayer = (int)CollisionLayer.Ground}
                );
            activeTiledMap.CreateObjects();
            CreateEntity("test-item")
                .SetPosition(180, 160)
                .AddComponent(ItemLoader.LoadItem(ContentPath.Serializables.Items.Example_item_item));
                //.AddComponent(new ProjectileHomingEffect() { HomingRange = 100, HomingSpeed = 0.08f });

            _inputManager = Core.GetGlobalManager<InputManager>();
            foreach(var input in _inputManager.AvailablePlayers)
                AddPlayer(input);

            _inputManager.OnPlayerJoined += AddPlayer;

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new FollowCamera(FindComponentsOfType<DemoComponent>()[0].Entity));
        }

        void AddPlayer(PlayerInput input)
        {
            AddEntity(new Entity()).SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(activeTiledMap, input));
        }
    }
}
