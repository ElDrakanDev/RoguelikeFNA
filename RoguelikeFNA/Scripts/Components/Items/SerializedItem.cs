using Nez.ImGuiTools.ObjectInspectors;
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
        [BitmaskInspectable(typeof(ItemPool))] public int ItemPoolMask;
        
        public SerializedItem() { }
    }
}
