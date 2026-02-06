using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using RoguelikeFNA.Generation;
using RoguelikeFNA.Player;
using Nez.Sprites;
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

            _inputManager = Core.GetGlobalManager<InputManager>();
            _inputManager.OnPlayerJoined += AddPlayer;
            foreach (var input in _inputManager.AvailablePlayers)
                AddPlayer(input);
            var playerFollow = CreateEntity("PlayerFollow").AddComponent(new PlayerFollow());
            var cam = FindEntity("camera");
            cam.GetComponent<Camera>().SetZoom(1);
            cam.AddComponent(new TiledCamera(playerFollow.Entity));
        }

        void AddPlayer(PlayerInput input)
        {
            var atlas = Content.LoadSpriteAtlas(ContentPath.Atlases.Zero.Zero_atlas);
            var animator = new SpriteAnimator(){LocalOffset = new Vector2(20, 0)};
            animator.AddAnimationsFromAtlas(atlas);
            AddEntity(new Entity())
                .SetTag((int)Tag.Player)
                .SetLocalPosition(FindEntity("camera").Position)
                // .AddComponent(new DemoComponent(input))
                .AddComponent(new AllrounderPlayerController(input))
                .AddComponent(animator)
                .AddComponent(new PlatformerMover());
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
            
            if(Core.DebugRenderEnabled && Input.RightMouseButtonPressed)
            {
                var player = FindEntitiesWithTag((int)Tag.Player);
                if(player.Count == 0)
                    return;
                var nodes = navigator.ActiveRoom.FindPath(player[0].Position, Camera.MouseToWorldPoint());
                foreach(var node in nodes)
                    Debug.DrawPixel(node, 4, Color.Cyan, 3f);
            }
        }
    }
}
