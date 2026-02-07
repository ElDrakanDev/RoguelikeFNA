using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;
using Nez.Sprites;
using RoguelikeFNA.Utils;

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
        [Inspectable] protected float _projectileSpeed = 150f;

        Entity _shootPos;
        Entity _slashPos;

        SerializedEntity _projectile;
        SerializedEntity _slash;

        SoundEffect _slashSfx;
        SoundEffect _dashSfx;
        SoundEffect _jumpSfx;
        SoundEffect _shootSfx;

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

            _dashState.OnStateBegin += ActivateDash;
            _idleState.OnStateBegin += DeactivateDash;
            _walkState.OnStateBegin += DeactivateDash;
            _groundedAttackState.OnStateBegin += DeactivateDash;
            _groundedAttackState.OnStateBegin += Slash;
            _groundedShootState.OnStateBegin += DeactivateDash;
            _airAttackState.OnStateBegin += Slash;
            _groundedShootState.OnStateBegin += Shoot;
            _airShootState.OnStateBegin += Shoot;
        }

        void Slash()
        {
            var proj = MeleeAttack(_slash, _slashPos.LocalPosition);
            SoundEffectManager.Play(_slashSfx);
        }

        void Shoot()
        {
            var proj = FireProjectile(
                _projectile, _shootPos.Position, Vector2.UnitX * FaceDirection.LookDirection * _projectileSpeed
            );
            SoundEffectManager.Play(_shootSfx);
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
            if(ApplyDashMultiplier)
                return;
            ApplyDashMultiplier = true;
            Entity.GetComponent<SpriteTrail>()?.EnableSpriteTrail();
            _speed *= _dashSpeedMultiplier;
            SoundEffectManager.Play(_dashSfx);
        }

        protected virtual void DeactivateDash() {
            if(!ApplyDashMultiplier) return;
            _speed /= _dashSpeedMultiplier;
            ApplyDashMultiplier = false;
            Entity.GetComponent<SpriteTrail>()?.DisableSpriteTrail();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _slashSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.SaberSlash_WAV);
            _dashSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.ZeroDash_WAV);
            _jumpSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.ZeroWalkJump_WAV);
            _shootSfx = Entity.Scene.Content.LoadSoundEffect(ContentPath.Audio.BusterShot_WAV);
            _projectile = Entity.Scene.Content.LoadNson<SerializedEntity>(ContentPath.Serializables.Entities.Bullet_nson);
            _slash = Entity.Scene.Content.LoadNson<SerializedEntity>(ContentPath.Serializables.Entities.Slash_nson);
            _shootPos = Entity.Scene.CreateEntity("shootPos")
                .SetParent(Transform)
                .SetLocalPosition(new Vector2(40, 0));
            _slashPos = Entity.Scene.CreateEntity("slashPos")
                .SetParent(Transform)
                .SetLocalPosition(new Vector2(40, 0));
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
            if(PlatformerMover.IsGrounded)
                return _groundedAttackState;
            else
                return _airAttackState;
        }

        public override CharacterState SpecialState()
        {
            if(PlatformerMover.IsGrounded)
                return _groundedShootState;
            else
                return _airShootState;
        }

        public override CharacterState DashState()
        {
            if(PlatformerMover.IsGrounded)
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
            return !_context.PlatformerMover.IsGrounded || _machine.ElapsedTimeInState >= DashDuration || _context.WantsAttack() || _context.WantsSpecial();
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