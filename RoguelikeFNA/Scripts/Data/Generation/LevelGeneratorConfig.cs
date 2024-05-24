using System.Collections.Generic;

namespace RoguelikeFNA.Generation
{
    [System.Serializable]
    public class LevelGenerationConfig
    {
        public string Name;
        public Dictionary<RoomTypes, int> RoomAmounts;
        public int NormalRoomVariance;
        public string RoomFilesDirectory;
    }
}
