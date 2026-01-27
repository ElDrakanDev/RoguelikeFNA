using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.FSM;
using Nez.Sprites;

namespace RoguelikeFNA.Player
{
    /// <summary>
    /// Base player controller using a small Nez StateMachine with four core states:
    /// Idle, Moving, Attack, Hurt.
    /// 
    /// This is intentionally minimal and meant to be extended.
    /// </summary>
    public abstract class BasePlayerController : Component, IUpdatable
    {
        [Inspectable] protected float _speed = 150f;
        [Inspectable] protected float _gravity = 900f;
        [Inspectable] protected float _jumpForce = 450f;
        [Inspectable] protected float _jumpBufferMax = 0.11f;

        [Inspectable] protected float _hurtDuration = 0.35f;

        [Inspectable] protected string _idleAnim = "zero_idle";
        [Inspectable] protected string _moveAnim = "zero_walk";
        [Inspectable] protected string _jumpAnim = "zero_jump_loop";
        [Inspectable] protected string _fallAnim = "zero_fall_loop";
        [Inspectable] protected string _hurtAnim = "";

        float _groundedBufferTime;
        float _jumpInputBufferTime;

        bool _hurtRequested;

        public PlayerInput Input { get; }

        public FaceDirection FaceDirection { get; protected set; }
        public PhysicsBody Body { get; protected set; }
        public PlatformerMover Mover { get; protected set; }
        public BoxCollider Collider { get; protected set; }

        // Optional: if present, the states will play animations.
        public SpriteAnimator Animator { get; protected set; }

        protected StateMachine<BasePlayerController> _machine;
        public StateMachine<BasePlayerController> Machine => _machine;
        public CharacterState CurrentState => (CharacterState)_machine.CurrentState;

        internal bool HurtRequested => _hurtRequested;
        internal void ClearHurtRequest() => _hurtRequested = false;

        protected IdleState _idleState;
        protected WalkState _walkState;
        protected JumpState _jumpState;
        protected FallState _fallState;
        protected HurtState _hurtState;

        public BasePlayerController(PlayerInput input)
        {
            Input = input;
        }

        protected virtual void SetupStates()
        {
            _idleState = new IdleState(_idleAnim);
            _walkState = new WalkState(_moveAnim);
            _jumpState = new JumpState(_jumpAnim);
            _fallState = new FallState(_fallAnim);
            _hurtState = new HurtState(_hurtAnim) { Duration = _hurtDuration };
        }

        protected virtual void SetupStateMachine()
        {
            _machine = new StateMachine<BasePlayerController>(this, _idleState);
            _machine.AddState(_idleState);
            _machine.AddState(_walkState);
            _machine.AddState(_jumpState);
            _machine.AddState(_fallState);
            _machine.AddState(_hurtState);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            FaceDirection = Entity.GetOrCreateComponent<FaceDirection>();
            Entity.AddComponent<EntranceTeleport>();

            // These are required for motion.
            Body = Entity.GetOrCreateComponent<PhysicsBody>();
            Mover = Entity.GetOrCreateComponent<PlatformerMover>();

            // Collider is required for PlatformerMover.
            Collider = Entity.GetComponent<BoxCollider>();
            if (Collider == null)
            {
                Collider = Entity.AddComponent(new BoxCollider(new Rectangle(-12, -20, 33, 40))
                {
                    PhysicsLayer = (int)CollisionLayer.Entity,
                    CollidesWithLayers = (int)CollisionLayer.Ground
                });
            }

            Animator = Entity.GetComponent<SpriteAnimator>();

            SetupStates();
            SetupStateMachine();
        }

        public virtual void Update()
        {
            _machine?.Update(Time.DeltaTime);
        }

        internal void UpdateJumpBuffers(float deltaTime, bool allowJumpInput)
        {
            _groundedBufferTime -= deltaTime;
            _jumpInputBufferTime -= deltaTime;

            if (allowJumpInput && (Input?.Jump.IsPressed ?? false))
                _jumpInputBufferTime = _jumpBufferMax;
        }

        internal void ApplyCommonVerticalPhysics()
        {
            if (Mover == null)
                return;

            // Ceiling bonk: cancel upward velocity
            if (Mover.State.Above && Body.Velocity.Y < 0)
                Body.Velocity.Y = 0;

            if (!Mover.IsGrounded)
            {
                Body.Velocity.Y += _gravity * Time.DeltaTime;
            }
            else
            {
                // Small downward force helps keep grounded
                Body.Velocity.Y = 0.02f;
                _groundedBufferTime = _jumpBufferMax;
            }
        }

        public CharacterState DefaultState()
        {
            if (Mover.IsGrounded)
            {
                if(WantsMove())
                    return _walkState;
                else
                    return _idleState;
            }
            else
            {
                if(Body.Velocity.Y < 0)
                    return _jumpState;
                else
                    return _fallState;
            }
        }

        public abstract CharacterState AttackState();
        public abstract CharacterState SpecialState();
        public abstract CharacterState DashState();

        internal void TryConsumeJump()
        {
            if (Mover == null || !CurrentState.CanJump())
                return;

            if (_groundedBufferTime > 0 && _jumpInputBufferTime > 0)
            {
                _jumpInputBufferTime = 0;
                _groundedBufferTime = 0;

                Body.Velocity.Y = -_jumpForce;
                Mover.SetLeftGround();
            }

            // Jump cut (short hop)
            if (!Mover.IsGrounded && Body.Velocity.Y < 0 && (Input?.Jump.IsReleased ?? false))
                Body.Velocity.Y *= 0.5f;
        }

        internal float ReadHorizontalInput() => Input?.Horizontal?.Value ?? 0;

        internal void ApplyHorizontalMovement(float input, float speedMultiplier = 1f)
        {
            Body.Velocity.X = _speed * speedMultiplier * input;
            FaceDirection?.CheckFacingSide(input);
        }
    
        internal void ApplyHorizontalMovement(float speedMultiplier = 1f)
        {
            var xInput = ReadHorizontalInput();
            ApplyHorizontalMovement(xInput, speedMultiplier);
        }

        internal void StopHorizontalMovement() => Body.Velocity.X = 0;

        internal bool WantsMove() => ReadHorizontalInput() != 0;

        internal bool WantsAnyAbility() => WantsAttack() || WantsSpecial() || WantsDash();
        internal bool WantsAttack() => Input?.Attack?.IsPressed ?? false;
        internal bool WantsSpecial() => Input?.Special?.IsPressed ?? false;
        internal bool WantsDash() => Input?.Dash?.IsPressed ?? false;
        internal bool WantsJump() => Input?.Jump?.IsPressed ?? false;
        internal bool WantsAnyAction() => WantsAnyAbility() || WantsJump() || WantsMove();

        internal void PlayAnimIfPossible(string anim)
        {
            if (Animator == null)
                return;

            if (!string.IsNullOrWhiteSpace(anim) && !Animator.IsAnimationActive(anim))
                Animator.Play(anim);
        }

        public void ChangeStateTo(CharacterState newState) => Machine.ChangeState(newState);
    }
}
