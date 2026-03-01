using System;
using Nez;
using Nez.Tiled;

namespace RoguelikeFNA.Entities
{
    [Serializable]
    /// <summary>
    /// Base class for game entities that have stats, health, collision, and movement.
    /// Provides common functionality for player characters, enemies, and other interactive entities.
    /// </summary>
    public class GameEntity : Component, IDeathListener
    {
        public HealthController HealthController { get; private set; }
        public EntityStats Stats { get; private set; }
        public IMover Mover { get; private set; }
        public Collider Collider { get; private set; }
        public PhysicsBody Body { get; private set; }

        public override void OnAddedToEntity()
        {
            // Get component references
            HealthController = Entity.GetComponent<HealthController>();
            Stats = Entity.GetComponent<EntityStats>();
            Mover = Entity.GetComponent<IMover>();
            Collider = Entity.GetComponent<Collider>();
            Body = Entity.GetComponent<PhysicsBody>();

            HealthController.DeathListeners.Add(this);

            GameEntityManager.RegisterEntity(this);
        }

        public override void OnRemovedFromEntity(){
            UnregisterEntity();
            HealthController.DeathListeners.Remove(this);
        }

        public virtual void OnDeath(DeathInfo deathInfo)
        {
            UnregisterEntity();
        }

        protected void UnregisterEntity() => GameEntityManager.UnregisterEntity(this);

        protected virtual IMover CreateMover() => new TiledMapMover(Parent.GetComponent<TiledMapRenderer>()?.CollisionLayer);

        /// <summary>
        /// Get the health manager for this entity.
        /// </summary>
        public HealthController GetHealthManager() => HealthController;

        /// <summary>
        /// Get the entity stats for this entity.
        /// </summary>
        public EntityStats GetStats() => Stats;

        /// <summary>
        /// Get the tiled map mover for this entity.
        /// </summary>
        public IMover GetMover() => Mover;

        /// <summary>
        /// Get the collider for this entity.
        /// </summary>
        public Collider GetCollider() => Collider;

        /// <summary>
        /// Deal damage to this entity.
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo) => HealthController?.Hit(damageInfo);

        /// <summary>
        /// Heal this entity.
        /// </summary>
        public void Heal(HealInfo healInfo) => HealthController?.Heal(healInfo);

        /// <summary>
        /// Check if this entity is alive.
        /// </summary>
        public bool IsAlive => HealthController?.IsAlive ?? false;
    }
}
