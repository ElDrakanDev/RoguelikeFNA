using Microsoft.Xna.Framework;
using Nez;
using Nez.Persistence;
using RoguelikeFNA.Items;
using RoguelikeFNA.Utils;

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

            _inputManager = Core.GetGlobalManager<InputManager>();
            foreach(var input in _inputManager.AvailablePlayers)
                AddPlayer(input);

            _inputManager.OnPlayerJoined += AddPlayer;

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new FollowCamera(FindComponentsOfType<DemoComponent>()[0].Entity));

            CreateEntity("random-normal-item")
                .SetLocalPosition(Vector2.One * 220)
                .AddComponent(Core.GetGlobalManager<ItemRepository>().GetRandomItemFromPool(ItemPool.Normal).Clone());

            //CreateEntity("test-serializable").AddComponent(new SerializableComponent());
        }

        void AddPlayer(PlayerInput input)
        {
            AddEntity(new Entity()).SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(activeTiledMap, input));
        }
    }
}
