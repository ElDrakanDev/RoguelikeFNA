using System;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace RoguelikeFNA.Player
{
    public class AllrounderPlayerController : BasePlayerController
    {
        [Inspectable] protected string _attackAnim = "zero_attack1";
        [Inspectable] protected string _attackAirAnim = "zero_attack_air";
        [Inspectable] protected string _shootAnim = "zero_buster";
        [Inspectable] protected string _shootAirAnim = "zero_buster_air";
        [Inspectable] protected string _dashAnim = "zero_dash";

        [Inspectable] protected float _dashSpeedMultiplier = 1.75f;

        protected AttackState _groundedAttackState;
        protected AttackState _airAttackState;  
        protected AttackState _groundedShootState;
        protected AttackState _airShootState;
        protected WalkState _dashState;
        public bool ApplyDashMultiplier = false;

        public AllrounderPlayerController(PlayerInput input) : base(input){}

        protected override void SetupStates()
        {
            base.SetupStates();
            _groundedAttackState = new GroundAttackState(_attackAnim, () => Input.Attack){ Duration = 0.4f};
            _airAttackState = new AirAttackState(_attackAirAnim, () => Input.Attack){ Duration = 0.4f};
            _groundedShootState = new GroundAttackState(_shootAnim, () => Input.Special){ Duration = 0.4f};
            _airShootState = new AirAttackState(_shootAirAnim, () => Input.Special){ Duration = 0.4f};
            _dashState = new DashState(_dashAnim);

            _idleState.OnStateBegin += DeactivateDash;
            _walkState.OnStateBegin += DeactivateDash;
            _groundedAttackState.OnStateBegin += DeactivateDash;
            _groundedShootState.OnStateBegin += DeactivateDash;
        }

        protected override void SetupStateMachine()
        {
            base.SetupStateMachine();
            _machine.AddState(_groundedAttackState);
            _machine.AddState(_airAttackState);
            _machine.AddState(_groundedShootState);
            _machine.AddState(_airShootState);
            _machine.AddState(_dashState);
        }

        public virtual void ActivateDash() {
            if(ApplyDashMultiplier) return;
            ApplyDashMultiplier = true;
            Entity.GetComponent<SpriteTrail>()?.SetEnabled(true);
            _speed *= _dashSpeedMultiplier;
        }

        protected virtual void DeactivateDash() {
            if(!ApplyDashMultiplier) return;
            _speed /= _dashSpeedMultiplier;
            ApplyDashMultiplier = false;
            Entity.GetComponent<SpriteTrail>()?.SetEnabled(false);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            var spriteTrail = Entity.AddComponent(new SpriteTrail());
            spriteTrail.SetInitialColor(new Color(0.5f, 0, 0, 0.5f)).SetFadeToColor(Color.Transparent);
            spriteTrail.SetFadeDuration(0.2f).SetMaxSpriteInstances(30).SetMinDistanceBetweenInstances(0.1f);
            spriteTrail.DisableSpriteTrail();
        }

        public override void Update()
        {
            if((_machine.CurrentState == _idleState || _machine.CurrentState == _walkState) && Input.Dash.IsPressed)
                ChangeStateTo(_dashState);
            base.Update();
        }

        public override CharacterState AttackState()
        {
            if(Mover.IsGrounded)
                return _groundedAttackState;
            else
                return _airAttackState;
        }

        public override CharacterState SpecialState()
        {
            if(Mover.IsGrounded)
                return _groundedShootState;
            else
                return _airShootState;
        }

        public override CharacterState DashState()
        {
            if(Mover.IsGrounded)
                return _dashState;
            else
                return null;
        }
    }

    public class DashState : WalkState
    {
        protected Func<VirtualButton> _dashButtonGetter;
        public float DashDuration = 0.75f;

        public DashState(string animationName) : base(animationName)
        {
            _dashButtonGetter = () => _context.Input.Dash;
        }

        public override bool WantsTransitionIn()
        {
            return base.WantsTransitionIn() && _dashButtonGetter().IsPressed;
        }

        public override bool WantsTransitionOut()
        {
            return !_context.Mover.IsGrounded || _machine.ElapsedTimeInState >= DashDuration || _context.WantsAttack() || _context.WantsSpecial();
        }

        public override void Begin()
        {
            base.Begin();
            if (_context is AllrounderPlayerController allrounder)
                allrounder.ActivateDash();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if(_context.ReadHorizontalInput() == 0)
            {
                var input = _context.FaceDirection.FacingRight ? 1 : -1;
                _context.ApplyHorizontalMovement(input, 1f);
            }
        }
    }
}