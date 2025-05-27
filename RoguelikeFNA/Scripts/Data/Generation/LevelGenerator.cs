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
        // class RoomSpot
        // {
        //     public Point Position;
        //     public int Neighbors;
        //     public float DistanceToCenter;

        //     public static RoomSpot GetSpotInLevel(Point position, Level level)
        //     {
        //         var distance = Vector2.Distance(new Vector2(position.X, position.Y), Vector2.Zero);
        //         int neighbors = 0;
        //         foreach (var dir in _directions)
        //             if (level.Rooms.ContainsKey(position + dir))
        //                 neighbors++;
        //         return new RoomSpot() { DistanceToCenter = distance, Neighbors = neighbors, Position = position };
        //     }
        // }
        Dictionary<RoomType, WeightedRandomGenerator<Room>> _roomGenerators = new();
        List<Room> Rooms = new();
        RNG rng;
        // Point _currentPos;
        // static Point[] _directions = new Point[] { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };
        RoomType[] _specialTypes = new RoomType[] { RoomType.Boss, RoomType.Treasure, RoomType.Shop };

        public LevelGenerator()
        {
            rng = Core.GetGlobalManager<RNGManager>().GetRNG(RNGManager.GeneratorTypes.Level);
        }

        public Level GenerateLevel(LevelGenerationConfig config)
        {
            var level = new Level();
            level.GenerationConfig = config;
            level.Name = config.Name;
            config.RoomAmounts.TryGetValue(RoomType.Normal, out int normalRooms);
            var availableRooms = GetAvailableRooms(config.RoomFilesDirectory);
            CreateRoom(level, availableRooms, RoomType.Start);
            CreateMinimumNormalRooms(level, availableRooms, normalRooms);
            CreateAdditionalRooms(level, availableRooms, config);
            CreateRoom(level, availableRooms, RoomType.Boss);
            return level;
        }

        Dictionary<RoomType, List<Room>> GetAvailableRooms(string roomFilesDir)
        {
            Dictionary<RoomType, List<Room>> rooms = new Dictionary<RoomType, List<Room>>();

            var roomFiles = new List<string>();
            var types = Enum.GetValues(typeof(RoomType)).Cast<RoomType>();

            foreach (var type in types)
            {
                rooms[type] = new List<Room>();
                var subdirectory = Path.Combine(roomFilesDir, Enum.GetName(typeof(RoomType), type));
                if (Directory.Exists(subdirectory))
                {
                    roomFiles.AddRange(Directory.GetFiles(subdirectory, "*.tmx").Where(Room.IsValidFilename));
                }
            }

            foreach (var roomFile in roomFiles)
            {
                var room = Room.FromFilePath(roomFile);
                rooms[room.RoomType].Add(room);
            }
            return rooms;
        }

        void CreateMinimumNormalRooms(Level level, Dictionary<RoomType, List<Room>> availableRooms, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                CreateRoom(level, availableRooms, RoomType.Normal);
            }
        }

        void CreateRoom(Level level, Dictionary<RoomType, List<Room>> availableRooms, RoomType type)
        {
            if (!_roomGenerators.TryGetValue(type, out var generator))
            {
                generator = new WeightedRandomGenerator<Room>(rng);
                foreach (var room in availableRooms[type])
                    generator.AddItem(room, room.Weight);
                _roomGenerators.Add(type, generator);
            }
            level.Rooms.Add(generator.GetRandom().Value);
        }

        void CreateAdditionalRooms(
            Level level, Dictionary<RoomType, List<Room>> availableRooms, LevelGenerationConfig config
        )
        {
            var types = new List<RoomType>();
            for (int i = 0; i < rng.Range(0, config.NormalRoomVariance); i++)
                types.Add(RoomType.Normal);

            types.AddRange(config.RoomsBeforeBoss);

            for (int i = 0; i < types.Count; i++)
            {
                CreateRoom(level, availableRooms, types[0]);
                types.RemoveAt(rng.Range(0, types.Count - i));
            }
        }
    }

    public class TestGeneratorComponent : Component, IUpdatable
    {
        public bool UpdateOnPause { get; set; }

        Dictionary<RoomType, Color> _colors = new Dictionary<RoomType, Color>()
        {
            {RoomType.Normal, Color.White }, {RoomType.Shop, Color.DarkGreen }, {RoomType.Treasure, Color.Yellow}, {RoomType.Boss, Color.Red}
        };

        [Inspectable] Level _level;
        [Inspectable] string _name = "Test";
        [Inspectable] int _normalRoomVariance = 2;
        [Inspectable] Dictionary<RoomType, int> _roomAmounts = new Dictionary<RoomType, int>() {
            { RoomType.Normal, 10 }, { RoomType.Shop, 1 }, { RoomType.Treasure, 1 }, { RoomType.Boss, 1 }
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
            int offset = 30;
            int count = _level.Rooms.Count;
            int boxSize = 20;
            for (int i = 0; i < _level.Rooms.Count; i++)
                Debug.DrawHollowBox(
                    new Vector2(i * offset - count * offset / 2, 0) + Transform.Position, boxSize, _colors[_level.Rooms[i].RoomType]
                );
        }
    }
}