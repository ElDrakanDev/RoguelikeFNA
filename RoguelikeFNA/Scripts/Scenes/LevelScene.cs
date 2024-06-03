using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using RoguelikeFNA.Generation;
using RoguelikeFNA.Items;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    public class LevelScene : BaseScene
    {
        InputManager _inputManager;

        public override void Begin()
        {
            base.Begin();

            //activeTiledMap = CreateEntity("test-tilemap")
            //    .AddComponent(new TiledMapRenderer(
            //        Content.LoadTiledMap(ContentPath.Tilemaps.Mosaic.Tiled._0001_Mosaic_demo_tmx), "Walls_1", true)
            //    { RenderLayer = 1, PhysicsLayer = (int)CollisionLayer.Ground}
            //    );
            //activeTiledMap.CreateObjects();

            _inputManager = Core.GetGlobalManager<InputManager>();
            foreach(var input in _inputManager.AvailablePlayers)
                AddPlayer(input);

            _inputManager.OnPlayerJoined += AddPlayer;

            AddSceneComponent(new LevelNavigator())
                .SetLevel(new LevelGenerator().GenerateLevel(new LevelGenerationConfig() {
                    Name = "Mosaic",
                    NormalRoomVariance = 3,
                    RoomFilesDirectory = ContentPath.Tilemaps.Mosaic.Tiled.Directory,
                    RoomAmounts = new() {
                        { RoomTypes.Normal, 5 }, { RoomTypes.Shop, 1 }, { RoomTypes.Treasure, 1 }, { RoomTypes.Boss, 1 }
                    }
                })
            );

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new FollowCamera(FindComponentsOfType<DemoComponent>()[0].Entity));

            CreateEntity("random-normal-item")
                .SetLocalPosition(Vector2.One * 220)
                .AddComponent(Core.GetGlobalManager<ItemRepository>().GetRandomItemFromPool(ItemPool.Normal).Clone());


            //CreateEntity("test-serializable").AddComponent(new SerializableComponent());
            //CreateEntity("test-levelgenerator")
            //    .AddComponent(new TestGeneratorComponent());
        }

        void AddPlayer(PlayerInput input)
        {
            AddEntity(new Entity()).SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(input));
        }

        Dictionary<Keys, Point> _directions = new Dictionary<Keys, Point>()
        {
            { Keys.Left, new Point(-1, 0) }, { Keys.Up, new Point(0, -1)}, { Keys.Right, new Point(1, 0) }, { Keys.Down, new Point(0, 1)}
        };
        public override void Update()
        {
            base.Update();
            var navigator = GetSceneComponent<LevelNavigator>();
            foreach(var key in _directions.Keys)
                if (Input.IsKeyPressed(key))
                    navigator.MoveDirection(_directions[key]);
        }
    }
}
