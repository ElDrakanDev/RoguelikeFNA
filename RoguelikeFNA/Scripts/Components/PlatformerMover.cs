using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class PlatformerMover : Component, IMover
    {
        Collider _collider;
        public const float COLLISION_THRESHOLD = 0.01f;
        public const float BROADPHASE_PADDING = 10f;

        // If we were grounded last frame and are only moving down a tiny amount, push down a constant
        // distance so collision resolution keeps us snapped to the ground (helps with tiny gaps/slopes).
        public const float GROUNDED_GLUE_THRESHOLD = 0.05f;
        public const float GROUNDED_GLUE_DOWN_AMOUNT = 2f;

        public struct CollisionState
        {
            public bool Below;
            public bool Left;
            public bool Right;
            public bool Above;
        }

        public CollisionLayer CollisionLayer = CollisionLayer.Ground;
        public CollisionState State = new CollisionState();
        public CollisionState PreviousState = new CollisionState();

        public bool Vertical => State.Above || State.Below;
        public bool Horizontal => State.Left || State.Right;
        public bool IsGrounded => State.Below;
        public bool BecameGrounded => !PreviousState.Below && State.Below;
        public bool LeftGround => PreviousState.Below && !State.Below;
        public bool HitCeiling => !PreviousState.Above && State.Above;
        public bool IsOnWall => State.Left || State.Right;
        public bool BecameOnWall => (!PreviousState.Left || !PreviousState.Right) && IsOnWall;

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            Debug.WarnIf(_collider == null, "PlatformerMover has no Collider. PlatformerMover requires a Collider!");
        }

        public void SetLeftGround(){
            State.Below = false;
        }

        public bool Move(Vector2 motion)
        {
            if (_collider == null)
                return false;

            UpdatePreviousState();
            ResetState();

            // Ground glue: if we were grounded and are trying to move down by a very small amount,
            // increase the downward motion so we reliably collide with the ground and stay grounded.
            if (PreviousState.Below && motion.Y > 0 && motion.Y < GROUNDED_GLUE_THRESHOLD)
                motion.Y = GROUNDED_GLUE_DOWN_AMOUNT;

            bool collided = false;

            var bounds = _collider.Bounds;
            var endBounds = bounds;
            endBounds.X += motion.X;
            endBounds.Y += motion.Y;
            bounds = RectangleF.Union(bounds, endBounds);
            bounds.Inflate(BROADPHASE_PADDING, BROADPHASE_PADDING);
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, ref bounds, (int)CollisionLayer);
            // Move horizontally first
            if (motion.X != 0)
            {
                collided = MoveHorizontally(motion, neighbors);
            }

            // Then move vertically
            if (motion.Y != 0)
            {
                collided |= MoveVertically(motion, neighbors);
            }

            return collided;
        }

        private bool MoveVertically(Vector2 motion, HashSet<Collider> neighbors)
        {
            var collided = false;
            var verticalMotion = new Vector2(0, motion.Y);

            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsTrigger)
                    continue;

                if (_collider.CollidesWith(neighbor, verticalMotion, out CollisionResult result))
                {
                    // Set collision flags
                    if (result.MinimumTranslationVector.Y > 0)
                        State.Below = true;
                    if (result.MinimumTranslationVector.Y < 0)
                        State.Above = true;

                    verticalMotion.Y -= result.MinimumTranslationVector.Y;
                    collided = true;
                }
            }

            Entity.Transform.Position += verticalMotion;
            return collided;
        }

        private bool MoveHorizontally(Vector2 motion, HashSet<Collider> neighbors)
        {
            var collided = false;
            var horizontalMotion = new Vector2(motion.X, 0);

            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsTrigger)
                    continue;

                if (_collider.CollidesWith(neighbor, horizontalMotion, out CollisionResult result))
                {
                    // Set collision flags
                    if (result.MinimumTranslationVector.X < 0)
                        State.Left = true;
                    if (result.MinimumTranslationVector.X > 0)
                        State.Right = true;

                    horizontalMotion.X -= result.MinimumTranslationVector.X;
                    collided = true;
                }
            }

            Entity.Transform.Position += horizontalMotion;
            return collided;
        }

        void UpdatePreviousState()
        {
            PreviousState.Below = State.Below;
            PreviousState.Left = State.Left;
            PreviousState.Right = State.Right;
            PreviousState.Above = State.Above;
        }

        void ResetState()
        {
            State.Below = false;
            State.Left = false;
            State.Right = false;
            State.Above = false;
        }
    }
}