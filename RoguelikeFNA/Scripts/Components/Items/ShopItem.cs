using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Persistence;
using Nez.Sprites;

namespace RoguelikeFNA.Items
{
    [Serializable]
    public class ShopItem : Component, IInteractListener, IUpdatable
    {
        [NsonInclude] string ItemPath;
        [NsonInclude] ItemPool ItemPool = ItemPool.Shop;
        Item _item;
        public string Name => _item.Name;
        public string Desc => _item.Description;
        public int EssenceCost => _item.EssenceCost;
        public Texture2D Texture => _item.Texture;
        TextComponent _text;
        EssenceSceneComponent _essenceManager;

        public override void OnAddedToEntity()
        {
            _essenceManager = Entity.Scene.GetSceneComponent<EssenceSceneComponent>();
            var repo = Core.GetGlobalManager<ItemRepository>();
            if (!string.IsNullOrEmpty(ItemPath))
                _item = repo.LoadItem(ItemPath);
            else
                _item = repo.GetRandomItemFromPool(ItemPool);
            Entity.GetOrCreateComponent<SpriteRenderer>().SetTexture(Texture);
            Entity.GetOrCreateComponent<InteractableOutline>();
            Entity.GetOrCreateComponent<BoxCollider>().PhysicsLayer = (int)CollisionLayer.Interactable;
            _text = Entity.GetOrCreateComponent<TextComponent>();
            _text.Text = EssenceCost.ToString();
            _text.LocalOffset = new Vector2(0, -Texture.Height);
        }

        void IInteractListener.OnHover(Entity source)
        {
            _item.OnHover(source);
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
            if(_text != null)
                _text.Color = _essenceManager.CanAfford(EssenceCost) ? Color.White : Color.Red;
        }
    }
}