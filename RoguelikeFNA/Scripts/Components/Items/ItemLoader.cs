using Nez;

namespace RoguelikeFNA.Items
{
    public static class ItemLoader
    {
        public static Item LoadItem(string path)
        {
            var serializedItem = Core.Scene.Content.LoadNson<SerializedItem>(path);
            var texture = Core.Scene.Content.LoadTexture(serializedItem.TexturePath);
            var item = new Item() { ItemId = serializedItem.ItemId, Texture = texture, Effects = serializedItem.Effects};
            return item;
        }
    }
}
