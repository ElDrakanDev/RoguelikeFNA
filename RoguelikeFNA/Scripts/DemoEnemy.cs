using Nez;
using Nez.Particles;
using Nez.Sprites;

namespace RoguelikeFNA
{
    public class DemoEnemy : Component, IPrefab
    {
        public int Health = 10;
        HealthManager _healthManager;
        SpriteAnimator _animator;

        const string IDLE_ANIM = "enemy_idle";
        const string HIT_ANIM = "enemy_hit";

        public override void OnAddedToEntity()
        {
            _healthManager = Entity.GetComponent<HealthManager>();
            _animator = Entity.GetComponent<SpriteAnimator>();
            _animator.Play(IDLE_ANIM);
            _healthManager.onDeath += OnDeath;
            _animator.OnAnimationCompletedEvent += OnAnimationComplete;
            _healthManager.onDamageTaken += OnHit;
        }

        public override void OnRemovedFromEntity() => RemoveEventListeners();

        void RemoveEventListeners()
        {
            _healthManager.onDeath -= OnDeath;
            _healthManager.onDamageTaken -= OnHit;
            _animator.OnAnimationCompletedEvent -= OnAnimationComplete;
        }

        void OnHit(DamageInfo info)
        {
            if (info.Canceled is false) _animator.Play(HIT_ANIM, SpriteAnimator.LoopMode.Once);
        }

        void OnAnimationComplete(string anim)
        {
            if(anim == HIT_ANIM)
            {
                _animator.Play(IDLE_ANIM, SpriteAnimator.LoopMode.Loop);
            }
        }

        void OnDeath(object source)
        {
            var deathSound = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.EnemyExplode_WAV);
            Core.GetGlobalManager<SoundEffectManager>().Play(deathSound);
            Entity.Scene.CreateEntity("DeathEffect", Transform.Position)
                .AddComponent(new ParticleEmitter(Entity.Scene.Content.LoadParticleEmitterConfig(ContentPath.Particles.Explosion_pex)))
                .OnAllParticlesExpired += (particle) => particle.Entity.Destroy();
            Entity.Destroy();
        }

        public void AddComponents()
        {
            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Enemy.Enemy_atlas);
            _animator = Entity.AddComponent(new SpriteAnimator().AddAnimationsFromAtlas(atlas));
            Entity.AddComponent(new BoxCollider(28, 38) { PhysicsLayer = (int)CollisionLayer.Enemy, CollidesWithLayers = (int)CollisionLayer.Player });
            _healthManager = Entity.AddComponent(new HealthManager(Health));
        }
    }
}
