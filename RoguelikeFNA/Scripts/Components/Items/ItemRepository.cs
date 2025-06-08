using Nez;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoguelikeFNA.Items
{
    public class ItemRepository : GlobalManager
    {
        public Item[] Items { get; private set; }
        public RNG RNG => _rngManager.GetRNG(RNGManager.GeneratorTypes.Item);

        public List<Item> AvailableItems;

        RNGManager _rngManager;

        public override void OnEnabled()
        {
            base.OnEnabled();
            ReloadAllItems();
            _rngManager = Core.GetGlobalManager<RNGManager>();
        }

        /// <summary>
        /// Load all items in items folder and update the items array
        /// </summary>
        public void ReloadAllItems()
        {
            var enumerable = Directory.EnumerateFiles(ContentPath.Serializables.Items.Directory, "*.item");
            Items = enumerable.Select(LoadItem).ToArray();
            AvailableItems = Items.ToList();
        }

        public Item LoadItem(string path)
        {
            var serializedItem = Core.Content.LoadNson<SerializedItem>(path);
            var texture = Core.Content.LoadTexture(serializedItem.TexturePath);
            var item = new Item()
            {
                ItemId = serializedItem.ItemId,
                Texture = texture,
                Effects = serializedItem.Effects,
                ItemPoolMask = serializedItem.ItemPoolMask,
                EssenceCost = serializedItem.EssenceCost
            };
            return item;
        }

        public Item GetItemById(string id) => AvailableItems.First(item => item.ItemId == id);
        public Item PopItemById(string id)
        {
            var item = AvailableItems.First(item => item.ItemId == id);
            if (item == null)
            {
                AvailableItems.Remove(item);
                return item;
            }
            return null;
        }

        public Item GetRandomItemFromPool(ItemPool pool)
        {
            var itemsFromPool = AvailableItems.Where(item => Flags.IsFlagSet((int)item.ItemPoolMask, (int)pool)).ToArray();
            return (Item)itemsFromPool[RNG.Range(0, itemsFromPool.Length)].Clone();
        }

        public Item PopRandomItemFromPool(ItemPool pool)
        {
            var itemsFromPool = AvailableItems.Where(item => Flags.IsFlagSet((int)item.ItemPoolMask, (int)pool)).ToArray();
            var index = RNG.Range(0, itemsFromPool.Length);
            var item = AvailableItems[index];
            AvailableItems.RemoveAt(index);
            return (Item)item.Clone();
        }
    }
}