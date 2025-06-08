using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using RoguelikeFNA.Items;

namespace RoguelikeFNA.Prefabs
{
    public class ShopItemPrefab : Component, IPrefab, IInteractListener, IUpdatable
    {
        string ItemPath;
        ItemPool ItemPool = ItemPool.Shop;
        Item _item;
        public string Name => _item.Name;
        public string Desc => _item.Description;
        public int EssenceCost => _item.EssenceCost;
        public Texture2D Texture => _item.Texture;
        TextComponent _text;
        EssenceManager _essenceManager;
        bool IUpdatable.UpdateOnPause { get => false; set{} }

        void IPrefab.LoadPrefab()
        {
            var repo = Core.GetGlobalManager<ItemRepository>();
            if (!string.IsNullOrEmpty(ItemPath))
                _item = repo.LoadItem(ItemPath);
            else
                _item = repo.GetRandomItemFromPool(ItemPool);
            Entity.AddComponent(new SpriteRenderer(Texture));
            Entity.AddComponent(new InteractableOutline());
            Entity.AddComponent(new BoxCollider() { PhysicsLayer = (int)CollisionLayer.Interactable });
            _text = Entity.AddComponent(new TextComponent()
            {
                Text = EssenceCost.ToString(),
                LocalOffset = new Vector2(0, -Texture.Height)
            });
        }

        public override void OnAddedToEntity()
        {
            _essenceManager = Core.GetGlobalManager<EssenceManager>();
        }

        void IInteractListener.OnHover(Entity source)
        {

        }

        void IInteractListener.OnInteract(Entity source)
        {
            if (_essenceManager.TrySpendEssence(EssenceCost))
            {
                _item.OnInteract(source);
                Entity.Destroy();
            }
        }

        void IUpdatable.Update()
        {
            _text.Color = _essenceManager.CanAfford(EssenceCost) ? Color.White : Color.Red;
        }
    }
}