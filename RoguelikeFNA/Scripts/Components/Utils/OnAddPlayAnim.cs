using Nez;
using Nez.Sprites;

namespace RoguelikeFNA
{
    public class OnSpawnPlayAnim : Component
    {
        [Inspectable] public string AnimationName;
        [Inspectable] public SpriteAnimator.LoopMode LoopMode = SpriteAnimator.LoopMode.ClampForever;

        public override void OnAddedToEntity()
        {
            var animator = Entity.GetComponent<SpriteAnimator>();
            if (animator != null && !string.IsNullOrEmpty(AnimationName))
            {
                animator.Play(AnimationName, LoopMode);
            }
        }
    }
}