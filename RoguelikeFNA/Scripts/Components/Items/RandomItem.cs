using System;
using Nez;

namespace RoguelikeFNA.Items
{
    [Serializable]
    public class RandomItem : Component
    {
        public ItemPool ItemPools;

        public override void OnAddedToEntity()
        {
            var item = Core.GetGlobalManager<ItemRepository>().GetRandomItemFromPool(ItemPools);
            this.AddComponent(item);
        }
    }
}