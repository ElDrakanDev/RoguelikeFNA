using System;
using Nez;
using Nez.Particles;

namespace RoguelikeFNA.Entities
{
    [Serializable]
    public class DemoEnemy : GameEntity
    {
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            HealthController.onDeath += OnDeath;
        }

        public override void OnRemovedFromEntity()
        {
            HealthController.onDeath -= OnDeath;
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
    }
}
