using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeFNA.Generation
{

    [System.Serializable]
    public class Level
    {
        public string Name;
        public LevelGenerationConfig GenerationConfig;
        [NonSerialized] public List<Room> Rooms = new();
    }
}
