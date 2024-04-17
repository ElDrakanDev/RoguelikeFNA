using Nez;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeFNA.Items
{
    public class ItemRepository : GlobalManager
    {
        public Item[] Items { get; private set; }
        public List<Item> ItemPool;

        public override void OnEnabled()
        {
            base.OnEnabled();
            ReloadAllItems();
        }

        /// <summary>
        /// Load all items in items folder and update the items array
        /// </summary>
        public void ReloadAllItems()
        {
            var enumerable = Directory.EnumerateFiles(ContentPath.Serializables.Items.Directory, "*.item");
            Items = enumerable.Select(LoadItem).ToArray();
        }

        public Item LoadItem(string path)
        {
            var serializedItem = Core.Content.LoadNson<SerializedItem>(path);
            var texture = Core.Content.LoadTexture(serializedItem.TexturePath);
            var item = new Item() { ItemId = serializedItem.ItemId, Texture = texture, Effects = serializedItem.Effects };
            return item;
        }
    }
}
