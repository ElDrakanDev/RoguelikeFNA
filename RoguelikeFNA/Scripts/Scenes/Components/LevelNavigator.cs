using Nez;
using RoguelikeFNA.Generation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    public class LevelNavigator : SceneComponent
    {
        Level _level;
        int _currentIdx;
        public Room CurrentRoom => _level.Rooms[_currentIdx];
        public Entity CurrentTilemap => _tiledmapEntities[_currentIdx];
        Dictionary<int, Entity> _tiledmapEntities = new Dictionary<int, Entity>();

        public Entity ActiveTiledMap => _tiledmapEntities[_currentIdx];
        public event Action<Entity> OnRoomChanged;
        public Room[] Rooms => _level.Rooms.ToArray();

        public LevelNavigator()
        {
            OnRoomChanged += PerishableCleanup;
        }

        #region Public API

        public Entity GetActiveRoomEntity()
        {
            if (_tiledmapEntities.TryGetValue(_currentIdx, out var entity))
                return entity;
            return null;
        }

        public LevelNavigator SetLevel(Level level)
        {
            foreach (var entity in _tiledmapEntities.Values)
                entity.Destroy();
            _tiledmapEntities.Clear();
            _level = level;

            for (int i = 0; i < _level.Rooms.Count; i++)
            {
                var room = _level.Rooms[i];
                var entity = Scene.CreateEntity(room.Name);
                var renderer = entity.AddComponent(new TiledMapRenderer(Scene.Content.LoadTiledMap(room.TiledMapPath), "ground")
                { RenderLayer = 1, PhysicsLayer = (int)CollisionLayer.Ground }
                 );
                renderer.CreateObjects();
                SetLayersToRender(renderer);
                entity.Enabled = false;
                _tiledmapEntities.Add(i, entity);
            }
            _currentIdx = 0;
            CurrentTilemap.Enabled = true;
            OnRoomChanged?.Invoke(_tiledmapEntities[_currentIdx]);
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

        public bool Move(int places)
        {
            var idx = _currentIdx + places;
            if (IsValidIndex(idx))
            {
                SetIndex(idx);
                return true;
            }
            return false;
        }

        #endregion

        void SetIndex(int idx)
        {
            if (
                _tiledmapEntities.TryGetValue(_currentIdx, out var entityFrom)
                && _tiledmapEntities.TryGetValue(idx, out var entityTo)
            )
            {
                entityFrom.Enabled = false;
                entityTo.Enabled = true;
                _currentIdx = idx;
                OnRoomChanged?.Invoke(_tiledmapEntities[_currentIdx]);
            }
        }

        bool IsValidIndex(int index)
        {
            return index >= 0 && index < _level.Rooms.Count;
        }

        void PerishableCleanup(Entity _)
        {
            Scene.FindComponentsOfType<IPerishable>().Cast<Component>().Select(c => c.Entity).Destroy();
        }
    }
}
