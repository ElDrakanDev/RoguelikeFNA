using Microsoft.Xna.Framework;
using Nez;
using RoguelikeFNA.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeFNA
{
    public class LevelNavigator : SceneComponent
    {
        Level _level;
        Point _currentPosition;
        Room _currentRoom;
        Dictionary<Point, Entity> _tiledmapEntities = new Dictionary<Point, Entity>();

        public Entity ActiveTiledMap => _tiledmapEntities[_currentPosition];
        public event Action<Entity> OnRoomChanged;

        #region Public API

        public Entity GetActiveRoomEntity()
        {
            if(_tiledmapEntities.TryGetValue(_currentPosition, out var entity))
                return entity;
            return null;
        }

        public LevelNavigator SetLevel(Level level)
        {
            foreach (var entity in _tiledmapEntities.Values)
                entity.Destroy();
            _tiledmapEntities.Clear();
            _level = level;
            foreach(var point in _level.Rooms.Keys)
            {
                var room = _level.Rooms[point];
                var entity = Scene.CreateEntity(room.Name);
                var renderer = entity.AddComponent(new TiledMapRenderer(Scene.Content.LoadTiledMap(room.TiledMapPath), "Walls_1")
                { RenderLayer = 1, PhysicsLayer = (int)CollisionLayer.Ground }
                 );
                renderer.CreateObjects();
                SetLayersToRender(renderer);
                entity.Enabled = false;
                _tiledmapEntities.Add(point, entity);
            }
            SetPosition(Point.Zero);
            return this;
        }

        void SetLayersToRender(TiledMapRenderer renderer)
        {
            string[] layerNames = renderer.TiledMap.Layers
                .Where(l => l.Name.Contains("_values") is false)
                .Select(l => l.Name)
                .ToArray();

            renderer.SetLayersToRender(layerNames.ToArray());
        }

        public bool MoveDirection(Point direction)
        {
            var pos = _currentPosition + direction;
            if(IsValidDirection(direction) && _level.Rooms.ContainsKey(pos))
            {
                SetPosition(pos);
                return true;
            }
            return false;
        }

        public bool MoveDirection(Vector2 direction)
        {
            direction = Vector2.Normalize(direction);
            var directionPoint = new Point(Mathf.RoundToInt(direction.X), Mathf.RoundToInt(direction.Y));
            return MoveDirection(directionPoint);
        }

        public bool MovePosition(Point position)
        {
            if (_level.Rooms.TryGetValue(position, out var room))
            {
                SetPosition(position);
                return true;
            }
            return false;
        }

        #endregion

        void SetPosition(Point point)
        {
            if (
                _tiledmapEntities.TryGetValue(_currentPosition, out var entityFrom)
                && _tiledmapEntities.TryGetValue(point, out var entityTo)
            )
            {
                entityFrom.Enabled = false;
                entityTo.Enabled = true;
                _currentPosition = point;
                _currentRoom = _level.Rooms[point];
                OnRoomChanged?.Invoke(_tiledmapEntities[_currentPosition]);
            }
        }

        bool IsValidDirection(Point direction)
        {
            return Math.Abs(direction.X) + Math.Abs(direction.Y) == 1;
        }
    }
}
