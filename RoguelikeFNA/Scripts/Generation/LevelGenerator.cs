using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Dictionary<RoomType, WeightedRandomGenerator<RoomData>> _roomGenerators = new();
        List<RoomData> Rooms = new();
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

        Dictionary<RoomType, List<RoomData>> GetAvailableRooms(string roomFilesDir)
        {
            Dictionary<RoomType, List<RoomData>> rooms = new Dictionary<RoomType, List<RoomData>>();

            var roomFiles = new List<string>();
            var types = Enum.GetValues(typeof(RoomType)).Cast<RoomType>();

            foreach (var type in types)
            {
                rooms[type] = new List<RoomData>();
                var subdirectory = Path.Combine(roomFilesDir, Enum.GetName(typeof(RoomType), type));
                if (Directory.Exists(subdirectory))
                {
                    roomFiles.AddRange(Directory.GetFiles(subdirectory, "*.tmx").Where(RoomData.IsValidFilename));
                }
            }

            foreach (var roomFile in roomFiles)
            {
                var room = RoomData.FromFilePath(roomFile);
                rooms[room.RoomType].Add(room);
            }
            return rooms;
        }

        void CreateMinimumNormalRooms(Level level, Dictionary<RoomType, List<RoomData>> availableRooms, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                CreateRoom(level, availableRooms, RoomType.Normal);
            }
        }

        void CreateRoom(Level level, Dictionary<RoomType, List<RoomData>> availableRooms, RoomType type)
        {
            if (!_roomGenerators.TryGetValue(type, out var generator))
            {
                generator = new WeightedRandomGenerator<RoomData>(rng);
                foreach (var room in availableRooms[type])
                    generator.AddItem(room, room.Weight);
                _roomGenerators.Add(type, generator);
            }
            level.Rooms.Add(generator.GetRandom().Value);
        }

        void CreateAdditionalRooms(
            Level level, Dictionary<RoomType, List<RoomData>> availableRooms, LevelGenerationConfig config
        )
        {
            var types = new List<RoomType>();
            var normalRooms = rng.Range(0, config.NormalRoomVariance);
            for (int i = 0; i < normalRooms; i++)
                types.Add(RoomType.Normal);

            var specialRooms = new Dictionary<RoomType, int>(config.RoomAmounts);
            specialRooms.Remove(RoomType.Normal);
            specialRooms.Remove(RoomType.Boss);

            foreach (var key in specialRooms.Keys)
                for (int i = 0; i < specialRooms[key]; i++)
                    types.Add(key);

            types.Sort((t1, t2) => rng.Range(0, types.Count));

            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                CreateRoom(level, availableRooms, type);
            }
        }
    }
}