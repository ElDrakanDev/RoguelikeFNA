using System;
using Nez;
using Nez.Particles;

namespace RoguelikeFNA.Entities
{
    [Serializable]
    public class ExplodeDeathFx : Component, IDeathListener
    {
        public override void OnAddedToEntity()
        {
            Entity.GetComponent<HealthController>().DeathListeners.Add(this);
        }

        public override void OnRemovedFromEntity()
        {
            Entity.GetComponent<HealthController>().DeathListeners.Remove(this);
        }

        public void OnDeath(DeathInfo deathInfo)
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
