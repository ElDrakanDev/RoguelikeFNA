using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    public class DemoComponent : Component, IUpdatable
    {
        HitboxHandler hitboxHandler;
        SpriteAnimator animator;
        bool _isAttacking = false;
        bool _facingRight;

        [Inspectable] float _speed = 200;
        [Inspectable] float _gravity = 20;
        [Inspectable] float _jumpForce = 8;
        [Inspectable] Vector2 _velocity;
        Vector2 _prevVel;

        const string ATTACK_ANIM1 = "zero_attack1";
        const string IDLE_ANIM = "zero_idle";
        const string WALK_ANIM = "zero_walk";
        const string JUMP_START_ANIM = "zero_jump_start";
        const string JUMP_LOOP_ANIM = "zero_jump_loop";
        const string FALL_START_ANIM = "zero_fall_start";
        const string FALL_LOOP_ANIM = "zero_fall_loop";
        const string ATTACK_AIR_ANIM = "zero_attack_air";

        VirtualAxis _moveInput;
        TiledMapMover _mover;
        TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        BoxCollider _collider;

        public DemoComponent(TiledMapRenderer tmxmap)
        {
            _mover = new TiledMapMover(tmxmap.CollisionLayer);
        }

        public override void OnAddedToEntity()
        {
            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Out_atlas);
            _facingRight = Entity.Scale.X >= 0;

            Entity.AddComponent(_mover);
            _collider = Entity.AddComponent(new BoxCollider(new Rectangle(-12, -20, 33, 40)){
                PhysicsLayer = (int)CollisionLayer.Player,
                CollidesWithLayers = (int)CollisionLayer.Ground });

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
            hitboxHandler.PhysicsLayer = (int)CollisionLayer.None;
            hitboxHandler.CollidesWithLayers = (int)CollisionLayer.Enemy;
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
            HandleStates();
        }

        void OnAnimationComplete(string anim)
        {
            if (anim == ATTACK_ANIM1 || anim == ATTACK_AIR_ANIM)
                _isAttacking = false;
        }

        void HandleStates()
        {
            var xInput = _moveInput.Value;
            _prevVel = _velocity;
            _velocity.X = 0;

            if (_collisionState.Below is false)
                _velocity.Y += _gravity * Time.DeltaTime;
            else
            {
                _velocity.Y = 0;
                if (_isAttacking is false && Input.IsKeyPressed(Keys.Space))
                    _velocity.Y = -_jumpForce;
            }

            // Attack cancelling
            if(animator.IsAnimationActive(ATTACK_AIR_ANIM) && _collisionState.Below)
            {
                _isAttacking = false;
            }

            // Attack1
            if (Input.IsKeyPressed(Keys.J) && _isAttacking is false && _collisionState.Below)
            {
                hitboxHandler.ClearCollisions();
                animator.Play(ATTACK_ANIM1, SpriteAnimator.LoopMode.ClampForever);
                _isAttacking = true;
                animator.Speed = 3;
            }
            // Air attack
            else if(
                (Input.IsKeyPressed(Keys.J) && _isAttacking is false && _collisionState.Below is false)
                || (_isAttacking is true && _collisionState.Below is false)
            )
            {
                // If just started
                if(_isAttacking is false)
                {
                    hitboxHandler.ClearCollisions();
                    _isAttacking = true;
                    animator.Play(ATTACK_AIR_ANIM, SpriteAnimator.LoopMode.ClampForever);
                    animator.Speed = 3;
                }
                _velocity.X = _speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
            }
            // Jump
            else if(_isAttacking is false && _velocity.Y < 0)
            {
                _velocity.X = _speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
                animator.Speed = 2;

                if (_prevVel.Y >= 0 && animator.IsAnimationActive(JUMP_START_ANIM) is false)
                    animator.Play(JUMP_START_ANIM, SpriteAnimator.LoopMode.ClampForever);
                else if (animator.IsAnimationActive(JUMP_START_ANIM) is true && animator.IsRunning is false)
                    animator.Play(JUMP_LOOP_ANIM);
            }
            // Fall
            else if (_isAttacking is false && _velocity.Y > 0)
            {
                _velocity.X = _speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
                animator.Speed = 2;

                if (_prevVel.Y <= 0 && animator.IsAnimationActive(FALL_START_ANIM) is false)
                    animator.Play(FALL_START_ANIM, SpriteAnimator.LoopMode.ClampForever);
                else if (animator.IsRunning is false) // if other animation is done
                    animator.Play(FALL_LOOP_ANIM);
            }
            // Idle
            else if (_isAttacking is false && xInput == 0 && animator.IsAnimationActive(IDLE_ANIM) is false && _collisionState.Below)
            {
                animator.Play(IDLE_ANIM);
                animator.Speed = 0.5f;
            }
            // Walk
            else if (_isAttacking is false && xInput != 0 && _collisionState.Below)
            {
                animator.Speed = 1;
                if (animator.IsAnimationActive(WALK_ANIM) is false)
                    animator.Play(WALK_ANIM);

                _velocity.X = _speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
            }

            _mover.Move(_velocity, _collider, _collisionState);
        }

        void CheckFacingSide(float xInput)
        {
            if ((xInput > 0 && _facingRight is false) || (xInput < 0 && _facingRight is true))
            {
                _facingRight = !_facingRight;
                Entity.Scale *= new Vector2(-1, 1);
            }
        }
    }
}
