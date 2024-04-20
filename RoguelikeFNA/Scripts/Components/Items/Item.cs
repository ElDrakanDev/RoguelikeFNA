using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using RoguelikeFNA.Items;

namespace RoguelikeFNA
{
    [System.Flags]
    public enum ItemPool
    {
        Normal = 1,
        Shop = 2,
        Boss = 4,
        Story = 8,
    }

    public class Item : Component, IInteractListener, IUpdatable
    {
        public string ItemId;
        public Texture2D Texture;
        public List<ItemEffect> Effects = new List<ItemEffect>();
        public string Name => TranslationManager.GetTranslation($"{ItemId}_name");
        public ItemPool ItemPoolMask;
        public string Description => TranslationManager.GetTranslation($"{ItemId}_desc");
        bool _hasBeenPickedUp = false;


        #region test
        public override void OnAddedToEntity()
        {
            Insist.IsNotNull(ItemId);
            if (_hasBeenPickedUp) return;
            Insist.IsNotNull(Texture);
            Entity.AddComponent(new SpriteRenderer(Texture));
            Entity.AddComponent(new InteractableOutline());
            Entity.AddComponent(new BoxCollider() { PhysicsLayer = (int)CollisionLayer.Interactable });
        }

        bool _logged = false;
        public void OnHover(Entity source)
        {
            if(_logged || ItemId == string.Empty || _hasBeenPickedUp) return;
            _logged = true;
            var additionalInfo = string.Join("\n", Effects.Select(e => e.GetDescription()).ToList());
            Debug.Log($"{Name}\n{Description}\n{additionalInfo}");
        }
        #endregion

        public void OnInteract(Entity source)
        {
            if (_hasBeenPickedUp) return;
            _hasBeenPickedUp = true;
            var itemClone = source.AddComponent((Item)Clone());
            //itemClone.Effects = Effects.Select(e => (ItemEffect)e.Clone()).ToList();
            foreach (var effect in itemClone.Effects)
                effect.OnPickup(source, itemClone);
            Entity.Destroy();
        }

        public void RemoveItem()
        {
            if (!_hasBeenPickedUp) return;
            _hasBeenPickedUp = false;

            foreach (var effect in Effects)
                effect.OnRemove();
            Entity.RemoveComponent(this);
        }

        public void Update()
        {
            if (_hasBeenPickedUp is false) return;

            foreach (var effect in Effects)
                effect.Tick();
        }

        public void FireEvent<TListener, TParam>(TParam param) where TListener : IEvent<TParam>
        {
            foreach (var effect in Effects)
            {
                if(effect is TListener listener)
                    listener.Fire(param);
            }
        }

        public override Component Clone()
        {
            var item = (Item)base.Clone();
            item.Effects = Effects.Select(e => (ItemEffect)e.Clone()).ToList();
            return item;
        }
    }
}
