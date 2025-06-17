using Nez;
using System;

namespace RoguelikeFNA.Items
{
    [Serializable]
    public abstract class ItemEffect : ICloneable
    {
        [field:NonSerialized] public Item ItemSource { get; private set; }
        [field:NonSerialized] public Entity Owner { get; private set; }
        public abstract string DescriptionId { get; }

        public virtual object Clone() => MemberwiseClone();

        /// <summary>
        /// Get translated description
        /// </summary>
        /// <returns></returns>
        public virtual string GetDescription()
        {
            return TranslationManager.TryGetTranslation(DescriptionId);
        }

        /// <summary>
        /// Called when the item has been picked up
        /// </summary>
        /// <param name="source"></param>
        public virtual void OnPickup(Entity source, Item item)
        {
            Owner = source;
            ItemSource = item;
        }

        /// <summary>
        /// Called when the item is removed from player
        /// </summary>
        public virtual void OnRemove()
        {
            Owner = null;
        }

        /// <summary>
        /// Called on every frame when the item has been picked up
        /// </summary>
        public virtual void Tick() { }
    }
}
