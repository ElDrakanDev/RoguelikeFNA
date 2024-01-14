using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    public class DemoComponent : Component, IUpdatable
    {
        SoundEffectManager _sfxManager;
        SoundEffect _slashSfx;
        SoundEffect _dashSfx;
        SoundEffect _jumpSfx;

        HitboxHandler _hitboxHandler;
        SpriteAnimator _animator;
        bool _isAttacking = false;
        bool _isDashing => (_collisionState.Below && _dashState && _dashTime > 0) || (!_collisionState.Below && _dashState);
        bool _dashState = false;
        bool _facingRight;
        float _dashTime;

        [Inspectable] float _speed = 150;
        [Inspectable] float _dashSpeed = 250;
        [Inspectable] float _dashDuration = 0.7f;
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
        const string DASH_ANIM = "zero_dash";

        PlayerInput _input;

        TiledMapMover _mover;
        TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        BoxCollider _collider;
        SpriteTrail _spriteTrail;

        public DemoComponent(TiledMapRenderer tmxmap)
        {
            _mover = new TiledMapMover(tmxmap.CollisionLayer);
        }

        void Test(InputEvent evt)
        {  
            var inpEvt = evt;
            var idx = inpEvt.GamePadIndex;
        }

        public override void OnAddedToEntity()
        {
            Input.Emitter.AddObserver(InputEventType.GamePadConnected, Test);
            Input.Emitter.AddObserver(InputEventType.GamePadDisconnected, Test);

            _sfxManager = Core.GetGlobalManager<SoundEffectManager>();
            _slashSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.SaberSlash_WAV);
            _dashSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.ZeroDash_WAV);
            _jumpSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.ZeroWalkJump_WAV);

            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Out_atlas);
            _facingRight = Entity.Scale.X >= 0;

            Entity.AddComponent(_mover);
            _collider = Entity.AddComponent(new BoxCollider(new Rectangle(-12, -20, 33, 40)){
                PhysicsLayer = (int)CollisionLayer.Player,
                CollidesWithLayers = (int)CollisionLayer.Ground });

            var child = new Entity();
            child.SetParent(Entity);
            _animator = child.AddComponent(new SpriteAnimator())
                    .AddAnimationsFromAtlas(atlas);
            _animator.Play(IDLE_ANIM);
            child.LocalPosition = new Vector2(20, 0);
            _animator.UpdateOrder = -1;
            Entity.Scene.AddEntity(child);
            _animator.OnAnimationCompletedEvent += OnAnimationComplete;

            _spriteTrail = child.AddComponent(new SpriteTrail());
            _spriteTrail.SetInitialColor(new Color(0.5f, 0, 0, 0.5f)).SetFadeToColor(Color.Transparent);
            _spriteTrail.SetFadeDuration(0.2f).SetMaxSpriteInstances(30).SetMinDistanceBetweenInstances(0.1f);
            _spriteTrail.DisableSpriteTrail();

            _hitboxHandler = child.AddComponent(new HitboxHandler());
            _hitboxHandler.PhysicsLayer = (int)CollisionLayer.None;
            _hitboxHandler.CollidesWithLayers = (int)CollisionLayer.Enemy;
            _hitboxHandler.AnimationsHitboxes = Entity.Scene.Content.LoadJson<Dictionary<string, List<HitboxGroup>>>(
                ContentPath.Hitboxes.Zero_hitboxes_json);
            _hitboxHandler.OnCollisionEnter += col => Debug.Log($"Collided with {col}");
            _hitboxHandler.Animator = _animator;

            _input = new PlayerInput();
        }

        public void Update()
        {
            HandleStates();
            var gpds = Input.GamePads;
            var gpd = gpds.ElementAtOrDefault(0);
            if (gpd != null)
            {
                var btn = gpd.GetFirstPressedButton();

            }
            gpds.Equals("");
        }

        void HandleStates()
        {
            var xInput = _input.Horizontal;
            _prevVel = _velocity;
            _velocity.X = 0;
            _dashTime -= Time.DeltaTime;

            if (_collisionState.Above is true && _velocity.Y < 0)
                _velocity.Y = 0;
            if (_collisionState.Below is false)
                _velocity.Y += _gravity * Time.DeltaTime;
            else
            {
                _velocity.Y = 0;
                if (_isAttacking is false && _input.Jump.IsPressed)
                {
                    _velocity.Y = -_jumpForce;
                    _sfxManager.Play(_jumpSfx);
                }
            }

            if(_collisionState.Below && _input.Special.IsPressed)
            {
                _dashTime = _dashDuration;
                _dashState = true;
                _isAttacking = false;
                _spriteTrail.EnableSpriteTrail();
                _sfxManager.Play(_dashSfx);
            }
            if (_input.Special.IsReleased)
                _dashTime = 0;
            if (_isDashing && _collisionState.BecameGroundedThisFrame || _collisionState.Below && _dashTime <= 0)
            {
                _dashTime = 0;
                _dashState = false;
                _spriteTrail.DisableSpriteTrail();
            }

            // Attack cancelling
            if(_animator.IsAnimationActive(ATTACK_AIR_ANIM) && _collisionState.Below)
            {
                _isAttacking = false;
            }

            float speed = _isDashing ? _dashSpeed : _speed;

            // Attack1
            if (_input.Attack.IsPressed && _isAttacking is false && _isDashing is false && _collisionState.Below)
            {
                _hitboxHandler.ClearCollisions();
                _animator.Play(ATTACK_ANIM1, SpriteAnimator.LoopMode.ClampForever);
                _isAttacking = true;
                _animator.Speed = 3;
                _sfxManager.Play(_slashSfx);
            }
            // Air attack
            else if(
                (_input.Attack.IsPressed && _isAttacking is false && _collisionState.Below is false)
                || (_isAttacking is true && _collisionState.Below is false)
            )
            {
                // If just started
                if(_isAttacking is false)
                {
                    _hitboxHandler.ClearCollisions();
                    _isAttacking = true;
                    _animator.Play(ATTACK_AIR_ANIM, SpriteAnimator.LoopMode.ClampForever);
                    _animator.Speed = 3;
                    _sfxManager.Play(_slashSfx);
                }
                _velocity.X = speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
            }
            // Jump
            else if(_isAttacking is false && _velocity.Y < 0)
            {
                _velocity.X = speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
                _animator.Speed = 2;

                if (_prevVel.Y >= 0 && _animator.IsAnimationActive(JUMP_START_ANIM) is false)
                    _animator.Play(JUMP_START_ANIM, SpriteAnimator.LoopMode.ClampForever);
                else if (_animator.IsAnimationActive(JUMP_START_ANIM) is true && _animator.IsRunning is false)
                    _animator.Play(JUMP_LOOP_ANIM);
            }
            // Fall
            else if (_isAttacking is false && _velocity.Y > 0 && _collisionState.Below is false)
            {
                _velocity.X = speed * xInput * Time.DeltaTime;
                CheckFacingSide(xInput);
                _animator.Speed = 2;

                if (_prevVel.Y <= 0 && _animator.IsAnimationActive(FALL_START_ANIM) is false)
                    _animator.Play(FALL_START_ANIM, SpriteAnimator.LoopMode.ClampForever);
                else if (_animator.IsRunning is false) // if other animation is done
                    _animator.Play(FALL_LOOP_ANIM);
            }
            // Dash
            else if(_isDashing && _collisionState.Below)
            {
                if (_animator.IsAnimationActive(DASH_ANIM) is false)
                    _animator.Play(DASH_ANIM);
                _animator.Speed = 2;
                CheckFacingSide(xInput);
                int side = _facingRight ? 1 : -1;
                _velocity.X = speed * side * Time.DeltaTime;
            }
            // Idle
            else if (
                _isAttacking is false && _isDashing is false
                && xInput == 0 && _animator.IsAnimationActive(IDLE_ANIM) is false && _collisionState.Below
            )
            {
                _animator.Play(IDLE_ANIM);
                _animator.Speed = 0.5f;
            }
            // Walk
            else if (_isAttacking is false && xInput != 0 && _collisionState.Below)
            {
                _animator.Speed = 1;
                if (_animator.IsAnimationActive(WALK_ANIM) is false)
                    _animator.Play(WALK_ANIM);

                _velocity.X = speed * xInput * Time.DeltaTime;
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

        void OnAnimationComplete(string anim)
        {
            if (anim == ATTACK_ANIM1 || anim == ATTACK_AIR_ANIM)
                _isAttacking = false;
        }
    }
}
