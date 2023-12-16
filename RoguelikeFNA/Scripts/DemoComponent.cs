using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    public class DemoComponent : Component, IUpdatable
    {
        HitboxHandler hitboxHandler;
        SpriteAnimator animator;
        bool _isAttacking = false;
        bool _facingRight;
        [Inspectable] float _speed = 300;
        const string ATTACK_ANIM1 = "zero_attack1";
        const string IDLE_ANIM = "zero_idle";
        const string WALK_ANIM = "zero_walk";
        VirtualAxis _moveInput;

        public override void OnAddedToEntity()
        {
            Entity.Transform.SetPosition(Vector2.One * 200);
            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Out_atlas);
            _facingRight = Entity.Scale.X >= 0;
            Entity.Scale *= 4;

            var child = new Entity();
            child.SetParent(Entity);
            animator = child.AddComponent(new SpriteAnimator())
                    .AddAnimationsFromAtlas(atlas);
            animator.Play(IDLE_ANIM);
            child.LocalPosition = new Vector2(20, 0);
            animator.UpdateOrder = -1;
            Entity.Scene.AddEntity(child);
            animator.OnAnimationCompletedEvent += OnAnimationComplete;
            hitboxHandler = child.AddComponent(new HitboxHandler());
            hitboxHandler.AnimationsHitboxes = Entity.Scene.Content.LoadJson<Dictionary<string, List<HitboxGroup>>>(
                ContentPath.Hitboxes.Zero_hitboxes_json);
            hitboxHandler.OnCollisionEnter += col => Debug.Log($"Collided with {col}");
            hitboxHandler.Animator = animator;
            _moveInput = new VirtualAxis(
                new VirtualAxis.Node[]{new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D)}
            );
        }

        public void Update()
        {
            var xInput = _moveInput.Value;

            if (Input.IsKeyPressed(Keys.J) && _isAttacking is false)
            {
                hitboxHandler.ClearCollisions();
                animator.Play(ATTACK_ANIM1, SpriteAnimator.LoopMode.ClampForever);
                _isAttacking = true;
                animator.Speed = 2;

            }
            else if (_isAttacking is false && xInput == 0 && animator.IsAnimationActive(IDLE_ANIM) is false)
            {
                animator.Play(IDLE_ANIM);
            }
            else if(_isAttacking is false && xInput != 0)
            {
                if(animator.IsAnimationActive(WALK_ANIM) is false)
                    animator.Play(WALK_ANIM);
                // Facing side logic
                Transform.Position += Time.DeltaTime * _speed * xInput * Vector2.UnitX;
                if((xInput >= 0 && _facingRight is false) || (xInput < 0 && _facingRight is true))
                {
                    _facingRight = !_facingRight;
                    Entity.Scale *= new Vector2(-1, 1);
                }
            }
        }

        void OnAnimationComplete(string anim)
        {
            if (anim == ATTACK_ANIM1)
            {
                _isAttacking = false;
                animator.Speed = 1;
            }
        }
    }
}
