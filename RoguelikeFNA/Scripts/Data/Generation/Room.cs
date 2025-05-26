using Nez;
using System;
using System.IO;
using System.Linq;
using System.Globalization;

namespace RoguelikeFNA.Generation
{
    [Serializable]
    public class Room
    {
        const char SEPARATOR = '_';
        const int EXPECTED_FILENAME_PARTS = 2;

        public readonly string TiledMapPath;

        public readonly string Name;
        public readonly int Weight;
        public readonly RoomType RoomType;

        public Room(string tiledMapPath, string name, int weight, RoomType roomType)
        {
            TiledMapPath = tiledMapPath;
            Name = name;
            Weight = weight;
            RoomType = roomType;
        }

        public static Room FromFilePath(string path)
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            var parts = filename.Split(SEPARATOR);

            if(parts.Length != EXPECTED_FILENAME_PARTS)
                throw new ArgumentException($"Expected {EXPECTED_FILENAME_PARTS} parts in filename, separated by '{SEPARATOR}'");

            var name = parts[0];
            var weight = int.Parse(parts[1], CultureInfo.InvariantCulture);
            var type = (RoomType)Enum.Parse(typeof(RoomType), Directory.GetParent(path).Name);
            return new Room(path, name, weight, type);
        }

        public static bool IsValidFilename(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);

            if (!Enum.GetNames(typeof(RoomType)).Contains(Directory.GetParent(path).Name))
                return false;

            int amount = 0;
            foreach(var c in name)
                if (c == SEPARATOR)
                    amount++;

            return amount == EXPECTED_FILENAME_PARTS - 1;
        }
    }
}
