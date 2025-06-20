using Nez;
using RoguelikeFNA.Items;

namespace RoguelikeFNA.Prefabs
{
    public class ItemPrefab : Component, IPrefab
    {
        public string ItemPath;

        void IPrefab.LoadPrefab()
        {
            var item = Core.GetGlobalManager<ItemRepository>().LoadItem(ItemPath);
            this.AddComponent(item);
        }
    }
}