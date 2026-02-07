using Nez;
using Nez.Tiled;

namespace RoguelikeFNA
{
    /// <summary>
    /// Base class for game entities that have stats, health, collision, and movement.
    /// Provides common functionality for player characters, enemies, and other interactive entities.
    /// </summary>
    public abstract class GameEntity : Component, IPrefab
    {
        [Inspectable] protected int Health = 10;
        [Inspectable] protected int Damage = 5;
        [Inspectable] protected EntityTeam Team = EntityTeam.Enemy;

        private bool _isInitialized = false;

        public HealthController HealthController { get; private set; }
        public EntityStats Stats { get; private set; }
        public IMover Mover { get; private set; }
        public Collider Collider { get; private set; }
        public PhysicsBody Body { get; private set; }

        public override void OnAddedToEntity()
        {   
            // Ensure prefab is loaded and components are initialized, even if
            // the entity is created without calling LoadPrefab directly.
            if (!_isInitialized)
            {
                LoadPrefab();
                _isInitialized = true;
            }

            // Get component references
            HealthController = Entity.GetComponent<HealthController>();
            Stats = Entity.GetComponent<EntityStats>();
            Mover = Entity.GetComponent<IMover>();
            Collider = Entity.GetComponent<Collider>();
            Body = Entity.GetComponent<PhysicsBody>();
        }

        public virtual void LoadPrefab()
        {
            HealthController = Entity.AddComponent(new HealthController(Health));
            Stats = Entity.AddComponent(new EntityStats(Damage) { Team = Team });
            Mover = (IMover)Entity.AddComponent((Component)CreateMover());
            Collider = Entity.AddComponent(PrefabCollider());
            Body = Entity.AddComponent(new PhysicsBody());
        }

        protected virtual IMover CreateMover() => new TiledMapMover(Parent.GetComponent<TiledMapRenderer>()?.CollisionLayer);

        protected abstract Collider PrefabCollider();

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

        public override void OnEnabled()
        {
            AdjustPositionToPreventGroundOverlap();
        }

        void AdjustPositionToPreventGroundOverlap()
        {
            // Adjust position to prevent overlapping with ground
            if (Collider == null || Collider.IsTrigger)
                return;

            var broadphase = Physics.BoxcastBroadphase(Collider.Bounds, (int)CollisionLayer.Ground);
            
            foreach (var collider in broadphase)
            {
                if (collider.IsTrigger)
                    continue;

                if (Collider.CollidesWith(collider, out var result))
                {
                    // Apply position adjustment to separate from ground
                    Transform.Position -= result.MinimumTranslationVector;
                }
            }
            
        }
    }
}
