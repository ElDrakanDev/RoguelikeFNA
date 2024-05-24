using Nez;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace RoguelikeFNA.Generation
{
    [Serializable]
    public class Room
    {
        const char SEPARATOR = ';';
        const int EXPECTED_FILENAME_PARTS = 3;

        public readonly string TiledMapPath;

        public readonly string Name;
        public readonly int Weight;
        public readonly RoomTypes RoomType;

        public Room(string tiledMapPath, string name, int weight, RoomTypes roomType)
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
            var type = (RoomTypes)Enum.Parse(typeof(RoomTypes), parts[2]);
            return new Room(path, name, weight, type);
        }

        public static bool IsValidFilename(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);

            int amount = 0;
            foreach(var c in name)
                if (c == SEPARATOR)
                    amount++;

            return amount == EXPECTED_FILENAME_PARTS - 1;
        }
    }
}
