using Microsoft.Xna.Framework;
using Nez;
using RoguelikeFNA.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace RoguelikeFNA.Generation
{
    public class LevelGenerator
    {
        class RoomSpot
        {
            public Point Position;
            public int Neighbors;
            public float DistanceToCenter;

            public static RoomSpot GetSpotInLevel(Point position, Level level)
            {
                var distance = Vector2.Distance(new Vector2(position.X, position.Y), Vector2.Zero);
                int neighbors = 0;
                foreach(var dir in _directions)
                    if(level.Rooms.ContainsKey(position + dir))
                        neighbors++;
                return new RoomSpot() { DistanceToCenter = distance, Neighbors = neighbors, Position = position };
            }
        }

        RNG rng;
        Point _currentPos;
        static Point[] _directions = new Point[] {new(-1, 0), new(1, 0), new(0, -1), new(0, 1)};
        RoomTypes[] _specialTypes = new RoomTypes[] { RoomTypes.Boss, RoomTypes.Treasure, RoomTypes.Shop };

        public LevelGenerator()
        {
            rng = Core.GetGlobalManager<RNGManager>().GetRNG(RNGManager.GeneratorTypes.Level);
        }

        public Level GenerateLevel(LevelGenerationConfig config)
        {
            var level = new Level();
            level.GenerationConfig = config;
            level.Name = config.Name;
            config.RoomAmounts.TryGetValue(RoomTypes.Normal, out int normalRooms);
            normalRooms += rng.Range(0, config.NormalRoomVariance);
            var availableRooms = GetAvailableRooms(config.RoomFilesDirectory);
            CreateNormalRooms(level, availableRooms, normalRooms);
            CreateSpecialRooms(level, availableRooms, config);
            return level;
        }

        Dictionary<RoomTypes, List<Room>> GetAvailableRooms(string roomFilesDir)
        {
            Dictionary<RoomTypes, List<Room>> rooms = new Dictionary<RoomTypes, List<Room>>();

            foreach (var type in Enum.GetValues(typeof(RoomTypes)).Cast<RoomTypes>())
                rooms[type] = new List<Room>();

            var roomFiles = Directory.GetFiles(roomFilesDir, "*.tmx")
                .Where(Room.IsValidFilename);

            foreach (var roomFile in roomFiles)
            {
                var room = Room.FromFilePath(roomFile);
                rooms[room.RoomType].Add(room);
            }
            return rooms;
        }

        void CreateNormalRooms(Level level, Dictionary<RoomTypes, List<Room>> availableRooms, int amount)
        {
            WeightedRandomGenerator<Room> generator = new WeightedRandomGenerator<Room>(rng);
            foreach (var room in availableRooms[RoomTypes.Normal])
                generator.AddItem(room, room.Weight);

            var item = generator.GetRandomItem();
            level.Rooms[_currentPos] = item.Value;

            while (amount - 1 > 0)
            {
                _currentPos += _directions[rng.Range(0, _directions.Length)];
                if (level.Rooms.ContainsKey(_currentPos))
                    continue;
                item = generator.GetRandomItem();
                level.Rooms[_currentPos] = item.Value;
                amount--;
            }
        }

        List<RoomSpot> GetAvailableSpots(Level level)
        {
            var spots = new List<RoomSpot>();
            HashSet<Point> points = new HashSet<Point>();

            foreach (var point in level.Rooms.Keys)
            {
                foreach(var direction in _directions)
                {
                    var neighbor = point + direction;
                    if (level.Rooms.ContainsKey(neighbor) || points.Contains(neighbor)) continue;
                    spots.Add(RoomSpot.GetSpotInLevel(neighbor, level));
                    points.Add(neighbor);
                }
            }

            return spots;
        }

        bool HasNeighborOfType(Level level, Point point, RoomTypes type)
        {
            foreach( var direction in _directions)
                if (level.Rooms.TryGetValue(point + direction, out var room) && room.RoomType == type)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets roomspot index in list without the requested neighbor type. If none exists, returns defaultIdx
        /// </summary>
        /// <param name="level"></param>
        /// <param name=""></param>
        /// <returns></returns>
        int SpotIndexWithoutNeighborType(Level level, IList<RoomSpot> spots, RoomTypes withoutType, int defaultIdx = -1)
        {
            for(var i = 0; i < spots.Count; i++)
                if (HasNeighborOfType(level, spots[i].Position, withoutType) is false)
                    return i;
            return defaultIdx;
        }

        void CreateSpecialRooms(
            Level level, Dictionary<RoomTypes, List<Room>> availableRooms, LevelGenerationConfig config
        )
        {
            var spots = GetAvailableSpots(level);
            spots.Sort((a, b) => b.DistanceToCenter.CompareTo(a.DistanceToCenter));

            foreach(var type in _specialTypes)
            {
                if (config.RoomAmounts.TryGetValue(type, out var amount) is false)
                    continue;
                    WeightedRandomGenerator<Room> generator = new WeightedRandomGenerator<Room>(rng);
                    foreach (var room in availableRooms[type])
                        generator.AddItem(room, room.Weight);

                for (var i = 0; i < amount; i++)
                {
                    var spotIndex = SpotIndexWithoutNeighborType(level, spots, type, 0);

                    var spot = spots[spotIndex];
                    spots.RemoveAt(spotIndex);
                
                    var item = generator.GetRandomItem();
                    level.Rooms[spot.Position] = item.Value;
                }
            }
        }
    }

    public class TestGeneratorComponent : Component, IUpdatable
    {
        public bool UpdateOnPause { get; set; }

        Dictionary<RoomTypes, Color> _colors = new Dictionary<RoomTypes, Color>()
        {
            {RoomTypes.Normal, Color.White }, {RoomTypes.Shop, Color.DarkGreen }, {RoomTypes.Treasure, Color.Yellow}, {RoomTypes.Boss, Color.Red}
        };

        [Inspectable] Level _level;
        [Inspectable] string _name = "Test";
        [Inspectable] int _normalRoomVariance = 2;
        [Inspectable] Dictionary<RoomTypes, int> _roomAmounts = new Dictionary<RoomTypes, int>() {
            { RoomTypes.Normal, 10 }, { RoomTypes.Shop, 1 }, { RoomTypes.Treasure, 1 }, { RoomTypes.Boss, 1 }
        };

        public override void Initialize()
        {
            base.Initialize();
            CreateLevel();
        }

        [InspectorCallable]
        void CreateLevel() => _level = TestGenerateLevel();

        Level TestGenerateLevel()
        {
            var config = new LevelGenerationConfig()
            {
                Name = _name,
                NormalRoomVariance = _normalRoomVariance,
                RoomAmounts = _roomAmounts,
                RoomFilesDirectory = ContentPath.Test.Generator.Directory
            };
            var generator = new LevelGenerator();
            var level = generator.GenerateLevel(config);
            return level;
        }

        public void Update()
        {
            Transform.Position = Entity.Scene.Camera.Position;
            Debug.DrawCircle(Transform.Position, Color.White, 10);
            foreach(var point in _level.Rooms.Keys)
                Debug.DrawHollowBox(
                    new Vector2(point.X * 30, point.Y * 30) + Transform.Position, 20, _colors[_level.Rooms[point].RoomType]
                );
        }
    }
}