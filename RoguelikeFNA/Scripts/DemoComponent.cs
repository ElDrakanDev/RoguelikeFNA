using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using RoguelikeFNA.Utils;
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
        SoundEffect _shootSfx;

        HitboxHandler _hitboxHandler;
        SpriteAnimator _animator;
        bool _isAttacking = false;
        bool _isDashing => (_characterController.IsGrounded && _dashState && _dashTime > 0) || (!_characterController.IsGrounded && _dashState);
        bool _dashState = false;
        float _dashTime;
        float _groundedBufferTime;
        float _jumpInputBufferTime;

        [Inspectable] float _jumpBufferMax = 0.11f;
        [Inspectable] Vector2 _shootOffset = Vector2.UnitX * 40;
        [Inspectable] Vector2 _shootOffsetAir = Vector2.UnitX * 35;
        [Inspectable] float _speed = 150;
        [Inspectable] float _dashSpeed = 250;
        [Inspectable] float _dashDuration = 0.7f;
        [Inspectable] float _gravity = 20;
        [Inspectable] float _jumpForce = 8;
        [Inspectable] Vector2 _velocity;
        [Inspectable] int _maxAmmo = 3;
        [Inspectable] int _ammo;
        [Inspectable] float _projectileVelocity = 150;
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
        const string SHOOT_ANIM = "zero_buster";
        const string SHOOT_AIR_ANIM = "zero_buster_air";

        BoxCollider _collider;
        SpriteTrail _spriteTrail;
        FaceDirection _fDir;

        EntityStats _stats;
        PlayerInput _input;
        Vector2 AimDirection => _fDir.FacingRight ? Vector2.UnitX : -Vector2.UnitX;

        SerializedEntity _projectile;

        CharacterController _characterController;
        CharacterController.CharacterCollisionState _collisionState => _characterController.collisionState;

        public DemoComponent(PlayerInput input)
        {
            _input = input;
        }

        public override void OnAddedToEntity()
        {
            _fDir = Entity.AddComponent(new FaceDirection());
            Entity.AddComponent<EntranceTeleport>();
            _sfxManager = Core.GetGlobalManager<SoundEffectManager>();
            _slashSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.SaberSlash_WAV);
            _dashSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.ZeroDash_WAV);
            _jumpSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.ZeroWalkJump_WAV);
            _shootSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.BusterShot_WAV);

            var atlas = Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Zero.Zero_atlas);

            _collider = Entity.AddComponent(new BoxCollider(new Rectangle(-12, -20, 33, 40)){
                PhysicsLayer = (int)CollisionLayer.Entity,
                CollidesWithLayers = (int)CollisionLayer.Ground });
            _characterController = Entity.GetComponent<CharacterController>();

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
            _hitboxHandler.HitboxLayers = (int)(CollisionLayer.Entity | CollisionLayer.Interactable);
            _hitboxHandler.AnimationsHitboxes = Entity.Scene.Content.LoadJson<Dictionary<string, List<HitboxGroup>>>(
                ContentPath.Serializables.Hitboxes.Zero_hitboxes_json);
            _hitboxHandler.OnCollisionEnter += OnHitOther;
            _hitboxHandler.Animator = _animator;

            Entity.AddComponent(new HealthManager(25)).onDeath += e => { if (e.Canceled is false) Entity.Destroy(); };
            _stats = Entity.AddComponent(new EntityStats(5) { Team = EntityTeam.Friendly });
            _ammo = _maxAmmo;

            _projectile = Entity.Scene.Content.LoadNson<SerializedEntity>(ContentPath.Serializables.Entities.Bullet_nson);
        }

        public void Update()
        {
            HandleStates();
            HandleInteractables();
        }

        void HandleInteractables()
        {
            var entities = Entity.Scene.Entities.EntitiesOfType<Entity>().Enabled().ToArray();
            entities = entities.Where(e =>
            {
                var collider = e.GetComponent<Collider>();
                if(collider is null)
                    return false;
                return collider.PhysicsLayer.IsFlagSet((int)CollisionLayer.Interactable) &&
                    collider.CollidesWith(_collider, out var _)
                    && e.HasComponent<IInteractListener>();
            }).ToArray();
            if(entities.Length > 0)
            {
                var entity = entities.Closest(Transform.Position);
                entity.GetComponents<IInteractListener>().ForEach(i => i.OnHover(Entity));
                if (_input.Interact.IsPressed)
                    entity.GetComponents<IInteractListener>().ForEach(i => i.OnInteract(Entity));
            }
        }

        void HandleStates()
        {
            var xInput = _input.Horizontal;
            _prevVel = _velocity;
            _velocity.X = 0;
            _dashTime -= Time.DeltaTime;
            _groundedBufferTime -= Time.DeltaTime;
            _jumpInputBufferTime -= Time.DeltaTime;

            // Check Jump Buffers
            if (_input.Jump.IsPressed)
                _jumpInputBufferTime = _jumpBufferMax;

            if (_isDashing && _collisionState.BecameGroundedThisFrame || _characterController.IsGrounded && _dashTime <= 0)
            {
                _dashTime = 0;
                _dashState = false;
                _spriteTrail.DisableSpriteTrail();
            }
            if(_characterController.IsGrounded && _input.Dash.IsPressed)
            {
                _dashTime = _dashDuration;
                _dashState = true;
                _isAttacking = false;
                _spriteTrail.EnableSpriteTrail();
                _sfxManager.Play(_dashSfx);
            }
            if (_input.Dash.IsReleased)
                _dashTime = 0;

            if (_collisionState.Above is true && _velocity.Y < 0)
                _velocity.Y = 0;

            if (_characterController.IsGrounded is false)
                _velocity.Y += _gravity * Time.DeltaTime;
            else
            {
                _velocity.Y = 0.02f;
                _groundedBufferTime = _jumpBufferMax;
            }
            
            if(_groundedBufferTime > 0 && _isAttacking is false && _input.Attack.IsPressed is false && _jumpInputBufferTime > 0)
            {
                _jumpInputBufferTime = 0;
                _groundedBufferTime = 0;
                _velocity.Y = -_jumpForce;
                _characterController.LeaveGround();
                _sfxManager.Play(_jumpSfx);
                _animator.Play(JUMP_START_ANIM, SpriteAnimator.LoopMode.Once);
            }

            if (_characterController.IsGrounded is false && _velocity.Y < 0 && _input.Jump.IsReleased)
                _velocity.Y *= 0.5f;

            // Attack cancelling
            if((_animator.IsAnimationActive(ATTACK_AIR_ANIM) || _animator.IsAnimationActive(SHOOT_AIR_ANIM)) && _characterController.IsGrounded)
            {
                _isAttacking = false;
                _hitboxHandler.ClearCollisions();
            }

            float speed = _isDashing ? _dashSpeed : _speed;

            // Attack1
            if (_input.Attack.IsPressed && _isAttacking is false && _characterController.IsGrounded)
            {
                _hitboxHandler.ClearCollisions();
                _animator.Play(ATTACK_ANIM1, SpriteAnimator.LoopMode.ClampForever);
                _isAttacking = true;
                _dashState = false;
                _animator.Speed = 3;
                _sfxManager.Play(_slashSfx);
            }
            // Air attack
            else if(
                (_input.Attack.IsPressed && _isAttacking is false && _characterController.IsGrounded is false)
                || (_isAttacking is true && _characterController.IsGrounded is false)
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
                _fDir.CheckFacingSide(xInput);
            }
            // Shoot
            else if(_input.Special.IsPressed && _isAttacking is false && _characterController.IsGrounded && _ammo > 0)
            {
                _animator.Play(SHOOT_ANIM, SpriteAnimator.LoopMode.ClampForever);
                _dashState = false;
                _animator.Speed = 2;
                Shoot(_shootOffset);
            }
            // Air shoot
            else if (
                (_input.Special.IsPressed && _isAttacking is false && _characterController.IsGrounded is false)
                || (_isAttacking is true && _characterController.IsGrounded is false)
            )
            {
                if(_isAttacking == false && _ammo > 0)
                {
                    _animator.Play(SHOOT_AIR_ANIM, SpriteAnimator.LoopMode.ClampForever);
                    _animator.Speed = 2;
                    Shoot(_shootOffsetAir);
                }
                _velocity.X = speed * xInput * Time.DeltaTime;
                _fDir.CheckFacingSide(xInput);
            }
            // Jump
            else if(_isAttacking is false && _velocity.Y < 0)
            {
                _velocity.X = speed * xInput * Time.DeltaTime;
                _fDir.CheckFacingSide(xInput);
                _animator.Speed = 2;

                if (_animator.IsAnimationActive(JUMP_START_ANIM) is true && _animator.IsRunning is false)
                    _animator.Play(JUMP_LOOP_ANIM);
            }
            // Fall
            else if (_velocity.Y > 0 && _characterController.IsGrounded is false)
            {
                _velocity.X = speed * xInput * Time.DeltaTime;
                _fDir.CheckFacingSide(xInput);
                _animator.Speed = 2;

                if (
                    _animator.IsAnimationActive(FALL_START_ANIM) is false
                    && _animator.IsAnimationActive(FALL_LOOP_ANIM) is false
                )
                    _animator.Play(FALL_START_ANIM, SpriteAnimator.LoopMode.Once);
                else if (_animator.IsRunning is false) // if other animation is done
                    _animator.Play(FALL_LOOP_ANIM);
            }
            // Dash
            else if(_isDashing && _characterController.IsGrounded)
            {
                if (_animator.IsAnimationActive(DASH_ANIM) is false)
                    _animator.Play(DASH_ANIM);
                _animator.Speed = 2;
                _fDir.CheckFacingSide(xInput);
                int side = _fDir.FacingRight ? 1 : -1;
                _velocity.X = speed * side * Time.DeltaTime;
            }
            // Idle
            else if (
                _isAttacking is false && _isDashing is false
                && xInput == 0 && _animator.IsAnimationActive(IDLE_ANIM) is false && _characterController.IsGrounded
            )
            {
                _animator.Play(IDLE_ANIM);
                _animator.Speed = 0.5f;
            }
            // Walk
            else if (_isAttacking is false && xInput != 0 && _characterController.IsGrounded)
            {
                _animator.Speed = 1;
                if (_animator.IsAnimationActive(WALK_ANIM) is false)
                    _animator.Play(WALK_ANIM);

                _velocity.X = speed * xInput * Time.DeltaTime;
                _fDir.CheckFacingSide(xInput);
            }

            _velocity = _characterController.Move(_velocity);
        }

        void OnAnimationComplete(string anim)
        {
            if (anim == ATTACK_ANIM1 || anim == ATTACK_AIR_ANIM || anim == SHOOT_ANIM || anim == SHOOT_AIR_ANIM)
                _isAttacking = false;
        }

        void OnHitOther(Entity other)
        {
            if(other.TryGetComponent(out EntityStats stats) && Flags.IsFlagSet(_stats.TargetTeams, (int)stats.Team))
            {
                stats.HealthManager.Hit(new DamageInfo(_stats[StatID.Damage], Entity));
                AddAmmo();
            }
        }

        void AddAmmo()
        {
            if (_ammo < _maxAmmo) _ammo += 1;
        }

        void Shoot(Vector2 offset)
        {
            _ammo -= 1;
            _isAttacking = true;
            _sfxManager.Play(_shootSfx);
            var entity = _projectile.AddToScene(Entity.Scene)
                .SetPosition(Transform.Position + offset * AimDirection)
                .SetLocalScale(Transform.LocalScale)
                .SetParent(Entity.Parent);
            //var anim = entity.AddComponent(new SpriteAnimator().AddAnimationsFromAtlas(Entity.Scene.Content.LoadSpriteAtlas(
            //    ContentPath.Atlases.Projectiles.Projectiles_atlas
            //)));
            //var proj = entity.AddComponent(new Projectile()
            //    { Velocity = AimDirection * _projectileVelocity, Damage = _stats.Damage, Lifetime = 5 });
            var proj = entity.GetComponent<Projectile>();
            proj.Velocity = AimDirection * _projectileVelocity;
            proj.SetValuesFromEntityStats(_stats);
            //anim.Play(anim.Animations.Keys.First());
            FireEvent<IProjectileShootListener, Projectile>(proj);
        }

        void FireEvent<TListener, TParam>(TParam param) where TListener : class, IEvent<TParam>
        {
            Entity.GetComponents<TListener>().ForEach(i => i.Fire(param));
            foreach (var item in Entity.GetComponents<Item>())
                item.FireEvent<TListener, TParam>(param);
        }
    }
}