using System;
using Nez;
using Nez.Sprites;
using Nez.AI.FSM;
using Microsoft.Xna.Framework;
using System.Linq;

namespace RoguelikeFNA.Entities
{
    public class StayState : State<GameEntity>
    {
        const string IDLE_ANIM = "enemy_idle";

        SpriteAnimator _animator;
        PhysicsBody _body;
        HitboxHandler _hitbox;
        DemoEnemyAI _ai;

        public override void OnInitialized()
        {
            _animator = _context.Entity.GetComponent<SpriteAnimator>();
            _body = _context.Entity.GetComponent<PhysicsBody>();
            _hitbox = _context.Entity.GetComponent<HitboxHandler>();
            _ai = _context.Entity.GetComponent<DemoEnemyAI>();
        }

        public override void Begin()
        {
            _animator.Play(IDLE_ANIM, SpriteAnimator.LoopMode.Loop);
            _body.Velocity.X = 0;
            _hitbox.ClearCollisions();
        }

        public override void Reason()
        {
            if (_ai.GetClosestTarget() != null)
            {
                _machine.ChangeState<ChaseState>();
            }
            else if (_machine.ElapsedTimeInState > _ai.StayTime)
            {
                var patrolState = _machine.GetState<PatrolState>();
                patrolState.SetTargetPosition(_ai.GetPointOnCurrentPlatform());
                _machine.ChangeState(patrolState);
            }
        }

        public override void Update(float deltaTime)
        {
        }
    }

    public class PatrolState : State<GameEntity>
    {
        const string WALK_ANIM = "enemy_move";

        Vector2 _movingTowards;
        SpriteAnimator _animator;
        PhysicsBody _body;
        BoxCollider _collider;
        FaceDirection _fDir;
        DemoEnemyAI _ai;

        public void SetTargetPosition(Vector2 target) => _movingTowards = target;

        public override void OnInitialized()
        {
            _animator = _context.Entity.GetComponent<SpriteAnimator>();
            _body = _context.Entity.GetComponent<PhysicsBody>();
            _collider = _context.Entity.GetComponent<BoxCollider>();
            _fDir = _context.Entity.GetComponent<FaceDirection>();
            _ai = _context.Entity.GetComponent<DemoEnemyAI>();
        }

        public override void Begin()
        {
            _animator.Play(WALK_ANIM, SpriteAnimator.LoopMode.Loop);
        }

        public override void Reason()
        {
            var transform = _context.Entity.Transform;

            // Arrived at target location or vertical axis is unaligned from target
            if (Vector2.Distance(transform.Position, _movingTowards) < 1 || 
                Math.Abs(_movingTowards.Y - transform.Position.Y) > 1)
            {
                _machine.ChangeState<StayState>();
            }
            else if (_ai.GetClosestTarget() is not null)
            {
                _machine.ChangeState<ChaseState>();
            }
        }

        public override void Update(float deltaTime)
        {
            var transform = _context.Entity.Transform;
            var moveInput = Math.Sign(_movingTowards.X - transform.Position.X);
            _fDir.CheckFacingSide(moveInput);
            _body.Velocity.X = moveInput * _ai.Speed;

            var rect = new RectangleF(_movingTowards - _collider.Bounds.Size / 2, _collider.Bounds.Size);
            Debug.DrawHollowRect(rect, Color.Yellow);
        }
    }

    public class ChaseState : State<GameEntity>
    {
        const string WALK_ANIM = "enemy_move";

        SpriteAnimator _animator;
        PhysicsBody _body;
        FaceDirection _fDir;
        HitboxHandler _hitbox;
        DemoEnemyAI _ai;

        public override void OnInitialized()
        {
            _animator = _context.Entity.GetComponent<SpriteAnimator>();
            _body = _context.Entity.GetComponent<PhysicsBody>();
            _fDir = _context.Entity.GetComponent<FaceDirection>();
            _hitbox = _context.Entity.GetComponent<HitboxHandler>();
            _ai = _context.Entity.GetComponent<DemoEnemyAI>();
        }

        public override void Begin()
        {
            _animator.Play(WALK_ANIM, SpriteAnimator.LoopMode.Loop);
            _hitbox.ClearCollisions();
        }

        public override void Reason()
        {
            var target = _ai.GetClosestTarget();
            if (target is null)
            {
                _machine.ChangeState<StayState>();
            }
        }

        public override void Update(float deltaTime)
        {
            var target = _ai.GetClosestTarget();
            if (target is null)
                return;

            var transform = _context.Entity.Transform;
            var moveInput = Math.Sign(target.Transform.Position.X - transform.Position.X);
            _fDir.CheckFacingSide(moveInput);
            _body.Velocity.X = moveInput * _ai.Speed;
        }
    }

    public class HurtState : State<GameEntity>
    {
        const string HIT_ANIM = "enemy_hit";

        SpriteAnimator _animator;
        PhysicsBody _body;

        public override void OnInitialized()
        {
            _animator = _context.Entity.GetComponent<SpriteAnimator>();
            _body = _context.Entity.GetComponent<PhysicsBody>();
        }

        public override void Begin()
        {
            _animator.Play(HIT_ANIM, SpriteAnimator.LoopMode.Once);
            _body.Velocity.X = 0;
        }

        public void OnAnimationComplete(string anim)
        {
            if (anim == HIT_ANIM)
                _machine.ChangeState<StayState>();
        }

        public override void Update(float deltaTime)
        {
        }
    }

    public class AttackState : State<GameEntity>
    {
        const string ATTACK_ANIM = "enemy_attack";

        SpriteAnimator _animator;
        PhysicsBody _body;
        HitboxHandler _hitbox;

        public override void OnInitialized()
        {
            _animator = _context.Entity.GetComponent<SpriteAnimator>();
            _body = _context.Entity.GetComponent<PhysicsBody>();
            _hitbox = _context.Entity.GetComponent<HitboxHandler>();
        }

        public override void Begin()
        {
            _animator.Play(ATTACK_ANIM, SpriteAnimator.LoopMode.Once);
            _body.Velocity.X = 0;
            _hitbox.ClearCollisions();
            _animator.Speed = 0.8f;
        }

        public void OnAnimationComplete(string anim)
        {
            if (anim == ATTACK_ANIM)
                _machine.ChangeState<StayState>();
        }

        public override void Update(float deltaTime)
        {
        }

        public override void End()
        {
            _hitbox.ClearCollisions();
            _animator.Speed = 1;
        }
    }
}
