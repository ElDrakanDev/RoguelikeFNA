using Nez;
using Nez.Particles;
using Nez.Sprites;
using System.Collections.Generic;

namespace RoguelikeFNA.Prefabs
{
    public class DemoEnemy : GameEntity
    {
        [Inspectable] int Essence = 25;

        public DemoEnemy()
        {
            Health = 10;
            Damage = 5;
            Team = EntityTeam.Enemy;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _healthManager.onDeath += OnDeath;
        }

        public override void OnRemovedFromEntity()
        {
            _healthManager.onDeath -= OnDeath;
        }

        void OnDeath(object source)
        {
            var deathSound = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.EnemyExplode_WAV);
            SoundEffectManager.Play(deathSound);
            Entity.Scene.CreateEntity("DeathEffect", Transform.Position)
                .AddComponent(new Perishable())
                .AddComponent(new ParticleEmitter(Entity.Scene.Content.LoadParticleEmitterConfig(ContentPath.Particles.Explosion_pex)))
                .OnAllParticlesExpired += (particle) => particle.Entity.Destroy();
            Entity.Destroy();
        }

        public override void LoadPrefab()
        {
            base.LoadPrefab();
            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Enemy.Enemy_atlas);
            var hitboxes = Entity.Scene.Content.LoadJson<Dictionary<string, List<HitboxGroup>>>(
                ContentPath.Serializables.Hitboxes.Demoenemy_hitbox_json);
            
            Entity.AddComponent(new SpriteAnimator().AddAnimationsFromAtlas(atlas));
            Entity.AddComponent(new FaceDirection());
            Entity.AddComponent(new HitboxHandler() { HitboxLayers = (int)CollisionLayer.Entity, AnimationsHitboxes = hitboxes });
            Entity.AddComponent(new EssenceOnDeath(Essence));
            Entity.AddComponent(new DemoEnemyAI());
        }

        protected override Collider PrefabCollider()
        {
            return new BoxCollider()
            {
                PhysicsLayer = (int)CollisionLayer.Entity,
                CollidesWithLayers = (int)CollisionLayer.Entity,
                Width = 28,
                Height = 38
            };
        }
    }
}
