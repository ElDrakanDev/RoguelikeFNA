using System;
using Nez;

namespace RoguelikeFNA.Items
{
    [Serializable]
    public class ItemWrapper : Component
    {
        public string ItemPath;

        public override void OnAddedToEntity()
        {
            var item = Core.GetGlobalManager<ItemRepository>().LoadItem(ItemPath);
            this.AddComponent(item);
        }
    }
}