using Nez;
using RoguelikeFNA.LevelElements;

namespace RoguelikeFNA.Utils
{
    public class PlatformGroundedMovement : Component, IUpdatable
    {
        PlatformerMover _mover;

        override public void OnAddedToEntity()
        {
            _mover = Entity.GetComponent<PlatformerMover>();
        }

        public void Update()
        {
            if (_mover.IsGrounded && _mover.GroundedOnPlatform)
            {
                var platform = _mover.State.StandingOn.Entity.GetComponent<MovingPlatform>();
                if (platform != null)
                {
                    Entity.Position += platform.DeltaMovement;
                }
            }
        }
    }
}