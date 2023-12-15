using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace RoguelikeFNA
{
    public class DemoComponent : Component, IUpdatable
    {
        HitboxHandler hitboxHandler;
        SpriteAnimator spriteAnimator;
        const string ATTACK_ANIM1 = "zero_attack1";
        const string IDLE_ANIM = "zero_idle1";

        public override void OnAddedToEntity()
        {
            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Out_atlas);

            spriteAnimator = Entity.SetLocalScale(2)
                .AddComponent(new SpriteAnimator())
                    .AddAnimationsFromAtlas(atlas);
            spriteAnimator.Play("zero_attack1");
            hitboxHandler = Entity.AddComponent(new HitboxHandler());
            hitboxHandler.OnCollisionEnter += col => Debug.Log($"Collided with {col}");
        }

        public void Update()
        {
            if (Input.IsKeyPressed(Keys.J))
            {
                hitboxHandler.ClearCollisions();
                spriteAnimator.Play(ATTACK_ANIM1);

            }
            //else if(spriteAnimator.IsRunning is false && spriteAnimator.CurrentAnimationName != IDLE_ANIM)
            //{
            //    spriteAnimator.Play(IDLE_ANIM);
            //}
        }
    }
}
