using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA.Generation
{
    public class LevelNavigator : SceneComponent
    {
        Level _level;
        int _currentIdx;
        public Room CurrentRoom => _rooms[_currentIdx];
        readonly Dictionary<int, Room> _rooms = new();

        public Room ActiveRoom => _rooms[_currentIdx];
        public event Action<Room> OnRoomChanged;
        public RoomData[] Rooms => _level.Rooms.ToArray();

        public LevelNavigator()
        {
            OnRoomChanged += PerishableCleanup;
        }

        #region Public API

        public LevelNavigator SetLevel(Level level)
        {
            foreach (var room in _rooms.Values)
                room.Entity.Destroy();
            _rooms.Clear();
            _level = level;

            for (int i = 0; i < _level.Rooms.Count; i++)
            {
                var room = new Room(_level.Rooms[i]);
                var entity = Scene.CreateEntity(room.Data.Name);
                entity.AddComponent(room);
                entity.Enabled = false;
                _rooms.Add(i, room);
            }
            _currentIdx = 0;
            CurrentRoom.Entity.Enabled = true;
            OnRoomChanged?.Invoke(_rooms[_currentIdx]);
            return this;
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
                _rooms.TryGetValue(_currentIdx, out var roomFrom)
                && _rooms.TryGetValue(idx, out var roomTo)
            )
            {
                roomFrom.Entity.Enabled = false;
                roomTo.Entity.Enabled = true;
                _currentIdx = idx;
                OnRoomChanged?.Invoke(_rooms[_currentIdx]);
            }
        }

        bool IsValidIndex(int index)
        {
            return index >= 0 && index < _level.Rooms.Count;
        }

        void PerishableCleanup(Room _)
        {
            Scene.FindComponentsOfType<IPerishable>()
                .Cast<Component>()
                .Select(c => c.Entity)
                .Where(e => !e.IsDestroyed)
                .ToHashSet()
                .Destroy();
        }
    }
}
