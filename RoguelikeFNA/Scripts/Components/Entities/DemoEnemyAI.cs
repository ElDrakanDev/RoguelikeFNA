using System;
using Nez;
using Nez.Sprites;
using Nez.AI.FSM;
using Nez.Tiled;
using Microsoft.Xna.Framework;
using System.Linq;

namespace RoguelikeFNA.Prefabs
{
    public enum PatrolStates
    {
        Stay, Patrol, Hurt, Chase, Attack
    }

    public class DemoEnemyAI : SimpleStateMachine<PatrolStates>
    {
        [Inspectable] float _gravity = 20;
        [Inspectable] float _speed = 40;
        [Inspectable] float _detectRange = 150f;
        [Inspectable] float _stayTime = 2f;

        Vector2 _movingTowards;

        EntityStats _stats;
        SpriteAnimator _animator;
        TiledMapMover _mover;
        BoxCollider _collider;
        TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
        HitboxHandler _hitbox;
        FaceDirection _fDir;
        HealthController _healthManager;
        PhysicsBody _body;

        const string IDLE_ANIM = "enemy_idle";
        const string HIT_ANIM = "enemy_hit";
        const string WALK_ANIM = "enemy_move";
        const string ATTACK_ANIM = "enemy_attack";

        public override void OnAddedToEntity()
        {
            _animator = Entity.GetComponent<SpriteAnimator>();
            _mover = Entity.GetComponent<TiledMapMover>();
            _collider = Entity.GetComponent<BoxCollider>();
            _fDir = Entity.GetComponent<FaceDirection>();
            _hitbox = Entity.GetComponent<HitboxHandler>();
            _stats = Entity.GetComponent<EntityStats>();
            _healthManager = Entity.GetComponent<HealthController>();
            _body = Entity.GetComponent<PhysicsBody>();
            
            _healthManager.onDamageTaken += OnHit;
            _animator.OnAnimationCompletedEvent += OnAnimationComplete;
            _hitbox.OnCollisionEnter += OnHitboxEnter;
            
            InitialState = PatrolStates.Stay;
        }

        public override void OnRemovedFromEntity()
        {
            _healthManager.onDamageTaken -= OnHit;
            _animator.OnAnimationCompletedEvent -= OnAnimationComplete;
            _animator.Play(IDLE_ANIM, SpriteAnimator.LoopMode.Loop);
            _hitbox.OnCollisionEnter -= OnHitboxEnter;
        }

        void OnHit(DamageInfo info)
        {
            if (info.Canceled is false)
                CurrentState = PatrolStates.Hurt;
        }

        void OnAnimationComplete(string anim)
        {
            if (anim == HIT_ANIM || anim == ATTACK_ANIM)
                CurrentState = PatrolStates.Stay;
        }

        public override void Update()
        {
            _body.Velocity.Y += _gravity * Time.DeltaTime;
            if (_collisionState.Below)
                _body.Velocity.Y = 0;
            _mover.Move(_body.Velocity, _collider, _collisionState);
            _fDir.CheckFacingSide(_body.Velocity.X);
            base.Update();
        }

        void OnHitboxEnter(Entity other)
        {
            // We detected an enemy inside our detection box. We change state to attack
            if (CurrentState == PatrolStates.Chase)
            {
                CurrentState = PatrolStates.Attack;
            }
            else if (CurrentState == PatrolStates.Attack
                && other.TryGetComponent(out EntityStats stats)
                && _stats.TargetTeams.IsFlagSet((int)stats.Team))
            {
                stats.HealthManager.Hit(new DamageInfo(_stats[StatID.Damage], Entity));
            }
        }

        #region AI States
        void Stay_Enter()
        {
            _animator.Play(IDLE_ANIM, SpriteAnimator.LoopMode.Loop);
            _body.Velocity.X = 0;
        }

        void Stay_Tick()
        {
            if (GetClosestTarget() != null)
                CurrentState = PatrolStates.Chase;
            else if (elapsedTimeInState > _stayTime)
            {
                CurrentState = PatrolStates.Patrol;
                _movingTowards = GetPointOnCurrentPlatform();
            }
        }

        void Patrol_Enter() => _animator.Play(WALK_ANIM, SpriteAnimator.LoopMode.Loop);

        void Patrol_Tick()
        {
            // Arrived at target location or vertical axis is unaligned from target
            if (Vector2.Distance(Transform.Position, _movingTowards) < 1 || Math.Abs(_movingTowards.Y - Transform.Position.Y) > 1)
            {
                CurrentState = PatrolStates.Stay;
                return;
            }
            else if (GetClosestTarget() is not null)
            {
                CurrentState = PatrolStates.Chase;
                return;
            }

            var moveInput = Math.Sign(_movingTowards.X - Transform.Position.X);
            _fDir.CheckFacingSide(moveInput);
            _body.Velocity.X = moveInput * _speed * Time.DeltaTime;

            var rect = new RectangleF(_movingTowards - _collider.Bounds.Size / 2, _collider.Bounds.Size);
            Debug.DrawHollowRect(rect, Color.Yellow);
        }

        void Chase_Enter()
        {
            _animator.Play(WALK_ANIM, SpriteAnimator.LoopMode.Loop);
        }

        void Chase_Tick()
        {
            var target = GetClosestTarget();
            if (target is null)
            {
                CurrentState = PatrolStates.Stay;
                return;
            }
            var moveInput = Math.Sign(target.Transform.Position.X - Transform.Position.X);
            _fDir.CheckFacingSide(moveInput);
            _body.Velocity.X = moveInput * _speed * Time.DeltaTime;
        }

        void Hurt_Enter()
        {
            _animator.Play(HIT_ANIM, SpriteAnimator.LoopMode.Once);
            _body.Velocity.X = 0;
        }

        void Attack_Enter()
        {
            _animator.Play(ATTACK_ANIM, SpriteAnimator.LoopMode.Once);
            _body.Velocity.X = 0;
            _hitbox.ClearCollisions();
            _animator.Speed = 0.8f;
        }

        void Attack_Exit()
        {
            _hitbox.ClearCollisions();
            _animator.Speed = 1;
        }

        #endregion

        EntityStats GetClosestTarget()
        {
            var enemies = Entity.Scene.FindComponentsOfType<EntityStats>()
                .Where(s => Flags.IsFlagSet(_stats.TargetTeams, (int)s.Team)
                    && s.InRange(Transform.Position, _detectRange)
                    && s.LineOfSight(Transform.Position)).ToList();

            Debug.DrawCircle(Transform.Position, Color.Yellow, _detectRange);

            if (enemies.Count > 0)
                return enemies.Closest(Transform.Position);
            return null;
        }

        Vector2 GetPointOnCurrentPlatform()
        {
            var ray = Physics.Linecast(
                Transform.Position, Transform.Position + Vector2.UnitY * _animator.Height, (int)CollisionLayer.Ground);
            var ground = ray.Collider;
            if (ground is not null)
            {
                var leftmost = ground.Bounds.Left + _collider.Width / 2;
                var rightmost = ground.Bounds.Right - _collider.Width / 2;
                var xpos = Transform.Position.X > (leftmost + rightmost) / 2 ? leftmost : rightmost;
                var point = new Vector2(xpos, Transform.Position.Y);
                return point;
            }
            // If no platform was found we stay in the same place
            return Transform.Position;
        }
    }
}
