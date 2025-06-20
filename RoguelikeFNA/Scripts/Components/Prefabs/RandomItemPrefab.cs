using Nez;
using RoguelikeFNA.Items;

namespace RoguelikeFNA.Prefabs
{
    public class RandomItemPrefab : Component, IPrefab
    {
        public ItemPool ItemPools;

        void IPrefab.LoadPrefab()
        {
            var item = Core.GetGlobalManager<ItemRepository>().GetRandomItemFromPool(ItemPools);
            this.AddComponent(item);
        }
    }
}