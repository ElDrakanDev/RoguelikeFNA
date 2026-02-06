using Nez;
using Nez.AI.Pathfinding;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeFNA.Generation
{
    [Serializable]
    public class Room : Component
    {
        public RoomData Data {get; private set; }
        public TiledMapRenderer TiledMapRenderer {get; private set; }
        AstarGridGraph _pathfindingGraph;

        public Room(RoomData data)
        {
            Data = data;
        }

        public override void Initialize()
        {
            var tiledmap = Entity.Scene.Content.LoadTiledMap(Data.TiledMapPath);
            TiledMapRenderer = Entity.AddComponent(new TiledMapRenderer(tiledmap, "ground"){
                RenderLayer = 1, PhysicsLayer = (int)CollisionLayer.Ground }
            );
            _pathfindingGraph = new AstarGridGraph(TiledMapRenderer.CollisionLayer);
            TiledMapRenderer.CreateObjects();
            SetLayersToRender();
        }

        void SetLayersToRender()
        {
            string[] layerNames = TiledMapRenderer.TiledMap.Layers
                .Where(l => l.Name.Contains("_values") is false)
                .Select(l => l.Name)
                .ToArray();

            TiledMapRenderer.SetLayersToRender(layerNames.ToArray());
        }

        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            var pointFrom = TiledMapRenderer.TiledMap.WorldToTilePosition(start);
            var pointTo = TiledMapRenderer.TiledMap.WorldToTilePosition(end);
            var path = _pathfindingGraph.Search(pointFrom, pointTo);
            return path.Select(TileToCenteredWorldPosition).ToList();
        }

        Vector2 TileToCenteredWorldPosition(Point tilePos)
        {
            return TiledMapRenderer.TiledMap.TileToWorldPosition(tilePos) + new Vector2(
                TiledMapRenderer.TiledMap.TileWidth * 0.5f,
                TiledMapRenderer.TiledMap.TileHeight * 0.5f
            );
        }
    }
}