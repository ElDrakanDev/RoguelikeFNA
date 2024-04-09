using System;
using System.Collections.Generic;


namespace RoguelikeFNA.Items
{
    [Serializable]
    public class SerializedItem
    {
        public string ItemId = string.Empty;
        public string TexturePath = string.Empty;
        public List<ItemEffect> Effects = new List<ItemEffect>();
        
        public SerializedItem() { }
    }
}
