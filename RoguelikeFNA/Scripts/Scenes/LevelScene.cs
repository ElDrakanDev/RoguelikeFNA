using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

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
                    Content.LoadTiledMap(ContentPath.Tilemaps.Mosaic.Tiled._0001_Mosaic_demo_tmx), "Walls_1", false)
                { RenderLayer = 1}
                );

            _inputManager = Core.GetGlobalManager<InputManager>();
            foreach(var input in _inputManager.AvailablePlayers)
                AddPlayer(input);

            _inputManager.OnPlayerJoined += AddPlayer;

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new FollowCamera(FindComponentsOfType<DemoComponent>()[0].Entity));


            var deathSound = Content.LoadSoundEffect(ContentPath.Audio.EnemyExplode_WAV);
            CreateEntity("test-collider")
               .SetLocalPosition(new Vector2(200, 200))
               .AddComponent(new BoxCollider(80, 80) { PhysicsLayer = (int)CollisionLayer.Enemy, CollidesWithLayers = (int)CollisionLayer.Player })
               .AddComponent(new HealthManager(25)).onDeath += dat => {
                   FindEntity("test-collider")?.Destroy(); Core.GetGlobalManager<SoundEffectManager>().Play(deathSound);
               };
        }

        void AddPlayer(PlayerInput input)
        {
            AddEntity(new Entity()).SetLocalPosition(new Vector2(200, 200)).AddComponent(new DemoComponent(_tmxMap, input));
        }
    }
}
