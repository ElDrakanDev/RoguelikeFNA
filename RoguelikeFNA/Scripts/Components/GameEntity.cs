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

        protected HealthManager _healthManager;
        protected EntityStats _stats;
        protected TiledMapMover _mover;
        protected Collider _collider;
        protected PhysicsBody _physicsBody;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            
            // Get component references
            _healthManager = Entity.GetComponent<HealthManager>();
            _stats = Entity.GetComponent<EntityStats>();
            _mover = Entity.GetComponent<TiledMapMover>();
            _collider = Entity.GetComponent<Collider>();
            _physicsBody = Entity.GetComponent<PhysicsBody>();
        }

        public virtual void LoadPrefab()
        {
            // Create health manager
            _healthManager = Entity.AddComponent(new HealthManager(Health));

            // Create entity stats
            _stats = Entity.AddComponent(new EntityStats(Damage) { Team = Team });

            _mover = Entity.AddComponent(new TiledMapMover(Parent.GetComponent<TiledMapRenderer>()?.CollisionLayer));

            _collider = Entity.AddComponent(PrefabCollider());
            _physicsBody = Entity.AddComponent(new PhysicsBody());
        }

        protected abstract Collider PrefabCollider();

        /// <summary>
        /// Get the health manager for this entity.
        /// </summary>
        public HealthManager GetHealthManager() => _healthManager;

        /// <summary>
        /// Get the entity stats for this entity.
        /// </summary>
        public EntityStats GetStats() => _stats;

        /// <summary>
        /// Get the tiled map mover for this entity.
        /// </summary>
        public TiledMapMover GetMover() => _mover;

        /// <summary>
        /// Get the collider for this entity.
        /// </summary>
        public Collider GetCollider() => _collider;

        /// <summary>
        /// Deal damage to this entity.
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo) => _healthManager?.Hit(damageInfo);

        /// <summary>
        /// Heal this entity.
        /// </summary>
        public void Heal(HealInfo healInfo) => _healthManager?.Heal(healInfo);

        /// <summary>
        /// Check if this entity is alive.
        /// </summary>
        public bool IsAlive => _healthManager?.IsAlive ?? false;

        public override void OnEnabled()
        {
            // Adjust position to prevent overlapping with ground
            if (_collider == null)
                return;

            var broadphase = Physics.BoxcastBroadphase(_collider.Bounds, (int)CollisionLayer.Ground);
            
            foreach (var collider in broadphase)
            {
                if (collider.IsTrigger)
                    continue;

                if (_collider.CollidesWith(collider, out var result))
                {
                    // Apply position adjustment to separate from ground
                    Transform.Position -= result.MinimumTranslationVector;
                }
            }
        }
    }
}
