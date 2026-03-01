using System;
using Nez;
using Nez.Sprites;
using Nez.AI.FSM;
using Microsoft.Xna.Framework;
using System.Linq;

namespace RoguelikeFNA.Entities
{
    [Serializable]
    public class DemoEnemyAI : Component, IUpdatable, IDamageListener
    {
        [Inspectable] public float Gravity = 20;
        [Inspectable] public float Speed = 40;
        [Inspectable] public float DetectRange = 150f;
        [Inspectable] public float StayTime = 2f;

        StateMachine<GameEntity> _stateMachine;
        
        EntityStats _stats;
        SpriteAnimator _animator;
        PlatformerMover _mover;
        BoxCollider _collider;
        HitboxHandler _hitbox;
        FaceDirection _fDir;
        HealthController _healthManager;
        PhysicsBody _body;
        GameEntity _gameEntity;

        const string IDLE_ANIM = "enemy_idle";

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _gameEntity = Entity.GetComponent<GameEntity>();
            _animator = Entity.GetComponent<SpriteAnimator>();
            _mover = Entity.GetComponent<PlatformerMover>();
            _collider = Entity.GetComponent<BoxCollider>();
            _fDir = Entity.GetComponent<FaceDirection>();
            _hitbox = Entity.GetComponent<HitboxHandler>();
            _stats = Entity.GetComponent<EntityStats>();
            _healthManager = Entity.GetComponent<HealthController>();
            _body = Entity.GetComponent<PhysicsBody>();
            
            // Create state machine with initial state
            _stateMachine = new StateMachine<GameEntity>(_gameEntity, new StayState());
            _stateMachine.AddState(new PatrolState());
            _stateMachine.AddState(new ChaseState());
            _stateMachine.AddState(new HurtState());
            _stateMachine.AddState(new AttackState());
            
            _healthManager.DamageListeners.Add(this);
            _animator.OnAnimationCompletedEvent += OnAnimationComplete;
            _hitbox.OnCollisionEnter += OnHitboxEnter;
        }

        public override void OnRemovedFromEntity()
        {
            _healthManager.DamageListeners.Add(this);
            _animator.OnAnimationCompletedEvent -= OnAnimationComplete;
            _animator.Play(IDLE_ANIM, SpriteAnimator.LoopMode.Loop);
            _hitbox.OnCollisionEnter -= OnHitboxEnter;
        }

        void OnAnimationComplete(string anim)
        {
            if (_stateMachine.CurrentState is HurtState hurtState)
            {
                hurtState.OnAnimationComplete(anim);
            }
            else if (_stateMachine.CurrentState is AttackState attackState)
            {
                attackState.OnAnimationComplete(anim);
            }
        }

        public void Update()
        {
            _body.Velocity.Y += Gravity * Time.DeltaTime;
            if (_mover.IsGrounded)
                _body.Velocity.Y = 0;
            _fDir.CheckFacingSide(_body.Velocity.X);
            
            _stateMachine.Update(Time.DeltaTime);
        }

        void OnHitboxEnter(Entity other)
        {
            // We detected an enemy inside our detection box. We change state to attack
            if (_stateMachine.CurrentState is ChaseState)
            {
                _stateMachine.ChangeState<AttackState>();
            }
            else if (_stateMachine.CurrentState is AttackState
                && other.TryGetComponent(out GameEntity gameEntity)
                && _stats.TargetTeams.IsFlagSet((int)gameEntity.Stats.Team))
            {
                gameEntity.HealthController.Hit(new (){Damage = Mathf.CeilToInt(_stats[StatID.Damage]), Source=Entity});
            }
        }

        public GameEntity GetClosestTarget()
        {
            var enemies = GameEntityManager.Entities
                .OfTeam(_stats.TargetTeams)
                .InRange(Transform.Position, DetectRange)
                .LineOfSight(Transform.Position)
                .ToList();

            Debug.DrawCircle(Transform.Position, Color.Yellow, DetectRange);

            if (enemies.Count > 0)
                return enemies.ClosestTo(Transform.Position);
            return null;
        }

        public Vector2 GetPointOnCurrentPlatform()
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

        public void OnDamageTaken(DamageInfo damageInfo)
        {
            _stateMachine.ChangeState<HurtState>();
        }
    }
}
