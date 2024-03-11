using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class LevelScene : BaseScene
    {
        InputManager _inputManager;
        TiledMapRenderer _tmxRenderer;

        public override void Initialize()
        {
            base.Initialize();

            _tmxRenderer = CreateEntity("test-tilemap")
                .AddComponent(new TiledMapRenderer(
                    Content.LoadTiledMap(ContentPath.Tilemaps.Mosaic.Tiled._0001_Mosaic_demo_tmx), "Walls_1", true)
                { RenderLayer = 1}
                );
            _tmxRenderer.CreateObjects();

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
            AddEntity(new Entity()).SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(_tmxRenderer, input));
        }
    }
}
