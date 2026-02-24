using Nez;
using Nez.AI.Pathfinding;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Tiled;

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
            _pathfindingGraph = CreatePathfindingGraph();
            TiledMapRenderer.CreateObjects();
            SetLayersToRender();
        }

        public override void OnEnabled()
        {
            foreach(var node in _pathfindingGraph.Walls)
            {
                var worldPos = TileToCenteredWorldPosition(node);
                Debug.DrawPixel(worldPos, 10, Color.Green, 5f);
            }
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
            if(path is null)
                return Enumerable.Empty<Vector2>().ToList();
            return path.Select(TileToCenteredWorldPosition).ToList();
        }

        Vector2 TileToCenteredWorldPosition(Point tilePos)
        {
            return TiledMapRenderer.TiledMap.TileToWorldPosition(tilePos) + new Vector2(
                TiledMapRenderer.TiledMap.TileWidth * 0.5f,
                TiledMapRenderer.TiledMap.TileHeight * 0.5f
            );
        }

        AstarGridGraph CreatePathfindingGraph()
        {
            var graph = new AstarGridGraph(TiledMapRenderer.TiledMap.Width, TiledMapRenderer.TiledMap.Height);
            var wallsLayer = (TmxLayer)TiledMapRenderer.TiledMap.GetLayer("ground");
            if (wallsLayer != null)
            {
                for (var y = 0; y < wallsLayer.Map.Height; y++)
                {
                    for (var x = 0; x < wallsLayer.Map.Width; x++)
                    {
                        var point = new Point(x, y);
                        if (wallsLayer.GetTile(x, y) != null || TileHasNeighbors(wallsLayer, point))
                            graph.Walls.Add(point);
                    }
                }
            }
            return graph;
        }

        bool TileHasNeighbors(TmxLayer layer, Point tile)
        {
            var neighbors = new Point[]
            {
                new Point(tile.X - 1, tile.Y),
                new Point(tile.X + 1, tile.Y),
                new Point(tile.X, tile.Y - 1),
                new Point(tile.X, tile.Y + 1)
            };

            return neighbors.Any(n => layer.GetTileSafe(n.X, n.Y) != null);
        }
    }
}