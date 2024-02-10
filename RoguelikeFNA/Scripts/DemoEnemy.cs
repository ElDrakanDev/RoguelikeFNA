using Nez;
using Nez.Sprites;

namespace RoguelikeFNA
{
    public class DemoEnemy : Component
    {
        HealthManager _healthManager;
        SpriteAnimator _animator;

        const string IDLE_ANIM = "enemy_idle";
        const string HIT_ANIM = "enemy_hit";

        public override void OnAddedToEntity()
        {
            var deathSound = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.EnemyExplode_WAV);
            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Enemy.Enemy_atlas);

            _animator = Entity.AddComponent(new SpriteAnimator().AddAnimationsFromAtlas(atlas));
            Entity.AddComponent(new BoxCollider(28, 38) { PhysicsLayer = (int)CollisionLayer.Enemy, CollidesWithLayers = (int)CollisionLayer.Player });

            _healthManager = Entity.AddComponent(new HealthManager(25));
            _healthManager.onDeath += dat => {
                Entity.Destroy(); Core.GetGlobalManager<SoundEffectManager>().Play(deathSound);
            };
            _animator.Play(IDLE_ANIM);
            _animator.OnAnimationCompletedEvent += OnAnimationComplete;
            _healthManager.onDamageTaken += info =>
            {
                if(info.Canceled is false) _animator.Play(HIT_ANIM, SpriteAnimator.LoopMode.Once);
            };
        }

        void OnAnimationComplete(string anim)
        {
            if(anim == HIT_ANIM)
            {
                _animator.Play(IDLE_ANIM, SpriteAnimator.LoopMode.Loop);
            }
        }
    }
}
