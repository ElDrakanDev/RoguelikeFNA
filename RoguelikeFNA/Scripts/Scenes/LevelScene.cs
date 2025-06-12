using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using RoguelikeFNA.Generation;
using RoguelikeFNA.Items;
using RoguelikeFNA.Rendering;
using RoguelikeFNA.UI;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    public class LevelScene : BaseScene
    {
        InputManager _inputManager;

        public override void Begin()
        {
            base.Begin();

            AddSceneComponent(new EssenceSceneComponent());

            CreateEntity("item-ui").AddComponent(new ItemTooltip());
            CreateEntity("essence-ui").AddComponent(new EssenceLabel());

            _inputManager = Core.GetGlobalManager<InputManager>();
            foreach (var input in _inputManager.AvailablePlayers)
                AddPlayer(input);

            _inputManager.OnPlayerJoined += AddPlayer;

            var nav = AddSceneComponent(new LevelNavigator())
                .SetLevel(new LevelGenerator().GenerateLevel(new LevelGenerationConfig() {
                    Name = "Mosaic",
                    NormalRoomVariance = 3,
                    RoomFilesDirectory = ContentPath.Tilemaps.Mosaic.Directory,
                    RoomAmounts = new() {
                        { RoomType.Normal, 5 }, { RoomType.Shop, 1 }, { RoomType.Treasure, 1 }
                    }
                })
            );

            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new TiledCamera(FindComponentsOfType<DemoComponent>()[0].Entity));
        }

        void AddPlayer(PlayerInput input)
        {
            AddEntity(new Entity())
                .SetLocalPosition(new Vector2(200, 200))
                .AddComponent(new DemoComponent(input))
                .AddComponent(new CharacterController());
        }

        Dictionary<Keys, int> _directions = new ()
        {
            { Keys.Left, -1 }, { Keys.Right, 1 }
        };
        public override void Update()
        {
            base.Update();
            var navigator = GetSceneComponent<LevelNavigator>();
            foreach(var key in _directions.Keys)
                if (Input.IsKeyPressed(key))
                    navigator.Move(_directions[key]);
        }
    }
}
