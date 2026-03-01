using System;
using Nez;
using Nez.AI.FSM;

namespace RoguelikeFNA.Player
{
    public abstract class CharacterState : State<BasePlayerController>
    {
        public bool CanMoveFreely {get; set;} = true;
        public event Action OnStateBegin;
        public event Action OnStateExit;

        public string AnimationName { get; protected set; }

        public StateMachine<BasePlayerController> Machine => _machine;

        protected CharacterState(string animationName)
        {
            AnimationName = animationName;
        }

        public virtual bool CanJump() => true;
        public virtual bool CanAttack() => true;
        public virtual bool CanUseSpecial() => true;

        public virtual bool WantsTransitionIn() => false;

        public virtual bool WantsTransitionOut() => true;

        protected virtual void TransitionFrom(CharacterState fromState) { }
        protected virtual void TransitionTo(CharacterState toState) { }

        public override void Begin()
        {
            _context.PlayAnimIfPossible(AnimationName);
            OnStateBegin?.Invoke();
        }

        public virtual CharacterState NextState()
        {
            if(_context.WantsAttack()){
                var attackState = _context.AttackState();
                if(attackState != null && attackState != this && attackState.WantsTransitionIn())
                    return attackState;
            }
            if(_context.WantsSpecial()){
                var specialState = _context.SpecialState();
                if(specialState != null && specialState != this && specialState.WantsTransitionIn())
                    return specialState;
            }
            if(_context.WantsDash()){
                var dashState = _context.DashState();
                if(dashState != null && dashState != this && dashState.WantsTransitionIn())
                    return dashState;
            }
            var defaultState = _context.DefaultState();
            if (defaultState != null && defaultState != this && defaultState.WantsTransitionIn())
                return defaultState;
            return null;
        }   

        public override void Reason()
        {
            if (!WantsTransitionOut())
                return;
            
            var nextState = NextState();
            if (nextState != null)
            {
                Machine.ChangeState(nextState);
                return;
            }
        }

        public override void End()
        {
            OnStateExit?.Invoke();
        }
    }

    public class IdleState : CharacterState
    {
        public IdleState(string idleAnim) : base(idleAnim) { }

        public override bool WantsTransitionIn()
        {
            return _context.PlatformerMover.IsGrounded && !_context.WantsAnyAction();
        }

        public override bool WantsTransitionOut()
        {
            return !_context.PlatformerMover.IsGrounded || _context.WantsAnyAction();
        }

        public override void Update(float deltaTime)
        {
            _context.UpdateJumpBuffers(deltaTime, allowJumpInput: true);
            _context.ApplyCommonVerticalPhysics();
            _context.StopHorizontalMovement();
            _context.TryConsumeJump();

            _context.PlayAnimIfPossible(AnimationName);
        }
    }

    public class WalkState : CharacterState
    {
        public WalkState(string walkAnim) : base(walkAnim) { }

        public override bool WantsTransitionIn()
        {
            return _context.PlatformerMover.IsGrounded && _context.WantsMove();
        }

        public override bool WantsTransitionOut()
        {
            return !_context.PlatformerMover.IsGrounded || !_context.WantsMove() || _context.WantsAnyAction();
        }

        public override void Update(float deltaTime)
        {
            _context.UpdateJumpBuffers(deltaTime, allowJumpInput: true);
            _context.ApplyCommonVerticalPhysics();
            _context.ApplyHorizontalMovement(1f);
            _context.TryConsumeJump();

            _context.PlayAnimIfPossible(AnimationName);
        }
    }

    public class JumpState : CharacterState
    {
        public JumpState(string jumpAnim) : base(jumpAnim) { }

        public override bool WantsTransitionIn()
        {
            return !_context.PlatformerMover.IsGrounded && _context.Body.Velocity.Y < 0;
        }

        public override void Update(float deltaTime)
        {
            _context.UpdateJumpBuffers(deltaTime, allowJumpInput: true);
            _context.ApplyCommonVerticalPhysics();
            _context.ApplyHorizontalMovement(1f);
            _context.TryConsumeJump();

            _context.PlayAnimIfPossible(AnimationName);
        }
    }

    public class FallState : CharacterState
    {
        public const float FALL_THRESHOLD = 0.05f;

        public FallState(string fallAnim) : base(fallAnim) { }

        public override bool WantsTransitionIn()
        {
            return !_context.PlatformerMover.IsGrounded && _context.Body.Velocity.Y > FALL_THRESHOLD;
        }

        public override void Update(float deltaTime)
        {
            _context.UpdateJumpBuffers(deltaTime, allowJumpInput: true);
            _context.ApplyCommonVerticalPhysics();
            _context.ApplyHorizontalMovement(1f);
            _context.TryConsumeJump();

            _context.PlayAnimIfPossible(AnimationName);
        }
    }

    public abstract class TimedState : CharacterState
    {
        public float Duration;
        protected float Elapsed;

        protected TimedState(string animationName) : base(animationName) { }

        public override void Begin()
        {
            base.Begin();
            Elapsed = 0f;
        }

        public override bool WantsTransitionOut() => Elapsed >= Duration;

        public override void Update(float deltaTime)
        {
            Elapsed += deltaTime;
        }
    }

    public class HurtState : TimedState
    {
        public HurtState(string hurtAnim) : base(hurtAnim) { CanMoveFreely = false; }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _context.StopHorizontalMovement();
        }
    }

    public abstract class AttackState : TimedState
    {
        protected Func<VirtualButton> _attackButtonGetter;
        protected AttackState(string animationName, Func<VirtualButton> virtualButtonGetter) : base(animationName)
        {
            _attackButtonGetter = virtualButtonGetter;
        }
    }

    public class GroundAttackState : AttackState
    {
        public GroundAttackState(string animationName, Func<VirtualButton> virtualButtonGetter) : base(animationName, virtualButtonGetter)
        {
            CanMoveFreely = false;
        }

        public override bool WantsTransitionOut()
        {
            return base.WantsTransitionOut() || !_context.PlatformerMover.IsGrounded;
        }

        public override bool WantsTransitionIn()
        {
            return _attackButtonGetter().IsPressed && _context.PlatformerMover.IsGrounded;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _context.StopHorizontalMovement();
        }
    }

    public class AirAttackState : AttackState
    {
        public AirAttackState(string animationName, Func<VirtualButton> virtualButtonGetter) : base(animationName, virtualButtonGetter) { }

        public override bool WantsTransitionOut()
        {
            return base.WantsTransitionOut() || _context.PlatformerMover.IsGrounded;
        }

        public override bool WantsTransitionIn()
        {
            return !_context.PlatformerMover.IsGrounded && _attackButtonGetter().IsPressed;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _context.UpdateJumpBuffers(deltaTime, allowJumpInput: false);
            _context.ApplyCommonVerticalPhysics();
            _context.ApplyHorizontalMovement(1f);
        }
    }
}