# define DEBUG_CC2D_RAYS

using System;
using System.Collections.Generic;
using Nez;
using Microsoft.Xna.Framework;

namespace RoguelikeFNA
{
    public class CharacterController : Component
    {
        #region internal types

        struct CharacterRaycastOrigins
        {
            public Vector2 topLeft;
            public Vector2 bottomRight;
            public Vector2 bottomLeft;
        }

        public class CharacterCollisionState
        {
            public bool Right;
            public bool Left;
            public bool Above;
            public bool Below;
            public bool BecameGroundedThisFrame;
            public bool WasGroundedLastFrame;
            public float SlopeAngle;


            public bool HasCollision()
            {
                return Below || Right || Left || Above;
            }


            public void Reset()
            {
                Right = Left = Above = Below = BecameGroundedThisFrame = false;
                SlopeAngle = 0f;
            }


            public override string ToString()
            {
                return string.Format("[CharacterCollisionState2D] r: {0}, l: {1}, a: {2}, b: {3}, angle: {4}, wasGroundedLastFrame: {5}, becameGroundedThisFrame: {6}",
                                        Right, Left, Above, Below, SlopeAngle, WasGroundedLastFrame, BecameGroundedThisFrame);
            }
        }

        #endregion


        #region events, properties and fields

        public event Action<RaycastHit> onControllerCollidedEvent;
        public event Action<Collider> onTriggerEnterEvent;
        public event Action<Collider> onTriggerStayEvent;
        public event Action<Collider> onTriggerExitEvent;


        [Inspectable] Vector2 checkPadding;

        /// <summary>
        /// when true, one way platforms will be ignored when moving vertically for a single frame
        /// </summary>
        public bool ignoreOneWayPlatformsThisFrame;

        [Inspectable]
        [Range(0.001f, 0.3f)]
        float _skinWidth = 0.02f;

        /// <summary>
        /// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
        /// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
        /// </summary>
        public float SkinWidth
        {
            get { return _skinWidth; }
            set
            {
                _skinWidth = value;
                recalculateDistanceBetweenRays();
            }
        }


        /// <summary>
        /// mask with all layers that the player should interact with
        /// </summary>
        public CollisionLayer platformMask = CollisionLayer.Ground;

        /// <summary>
        /// mask with all layers that trigger events should fire when intersected
        /// </summary>
        public CollisionLayer triggerMask = 0;

        /// <summary>
        /// mask with all layers that should act as one-way platforms. Note that one-way platforms should always be EdgeCollider2Ds. This is because it does not support being
        /// updated anytime outside of the inspector for now.
        /// </summary>
        [Inspectable]
        CollisionLayer oneWayPlatformMask = 0;

        /// <summary>
        /// the max slope angle that the CC2D can climb
        /// </summary>
        /// <value>The slope limit.</value>
        [Range(0f, 90f)]
        public float slopeLimit = 30f;

        /// <summary>
        /// the threshold in the change in vertical movement between frames that constitutes jumping
        /// </summary>
        /// <value>The jumping threshold.</value>
        public float jumpingThreshold = 0.07f;

        [Range(2, 20)]
        public int totalHorizontalRays = 8;
        [Range(2, 20)]
        public int totalVerticalRays = 5;

        float _slopeHighestPoint = 0;

        [NotInspectable]
        [NonSerialized]
        public BoxCollider boxCollider;

        [NotInspectable]
        [NonSerialized]
        public CharacterCollisionState collisionState = new CharacterCollisionState();
        [NotInspectable]
        [NonSerialized]
        public Vector2 velocity;
        public bool IsGrounded { get { return collisionState.Below || collisionState.SlopeAngle != 0; } }
        public bool IsOnSlope => collisionState.SlopeAngle != 0;

        const float kSkinWidthFloatFudgeFactor = 0.001f;

        const float EXTRA_SLOPE_RAY_DISTANCE = 4f;

        #endregion


        /// <summary>
        /// holder for our raycast origin corners (TR, TL, BR, BL)
        /// </summary>
        CharacterRaycastOrigins _raycastOrigins;

        /// <summary>
        /// stores our raycast hit during movement
        /// </summary>
        RaycastHit _raycastHit;

        /// <summary>
        /// stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
        /// horizontally and vertically so that we can send the events after all collision state is set
        /// </summary>
        List<RaycastHit> _raycastHitsThisFrame = new List<RaycastHit>(2);

        // horizontal/vertical movement data
        float _verticalDistanceBetweenRays;
        float _horizontalDistanceBetweenRays;

        // we use this flag to mark the case where we are travelling up a slope and we modified our delta.Y to allow the climb to occur.
        // the reason is so that if we reach the end of the slope we can make an adjustment to stay grounded
        bool _isGoingUpSlope = false;

        public void LeaveGround()
        {
            collisionState.Below = false;
            collisionState.SlopeAngle = 0;
        }


        #region Component Lifetime

        public override void OnAddedToEntity()
        {
            // add our one-way platforms to our normal platform mask so that we can land on them from above
            platformMask |= oneWayPlatformMask;

            // cache some components
            boxCollider = Entity.GetComponent<BoxCollider>();

            //rigidBody2D = GetComponent<Rigidbody2D>();

            // here, we trigger our properties that have setters with bodies
            SkinWidth = _skinWidth;

            //// we want to set our CC2D to ignore all collision layers except what is in our triggerMask
            //for (var i = 0; i < 32; i++)
            //{
            //    // see if our triggerMask contains this layer and if not ignore it
            //    if ((triggerMask.value & 1 << i) == 0)
            //        Physics2D.IgnoreLayerCollision(gameObject.layer, i);
            //}
            checkPadding = boxCollider.Bounds.Size / 2 - Vector2.One * SkinWidth;
        }


        public void OnTriggerEnter2D(Collider col)
        {
            if (onTriggerEnterEvent != null)
                onTriggerEnterEvent(col);
        }


        public void OnTriggerStay2D(Collider col)
        {
            if (onTriggerStayEvent != null)
                onTriggerStayEvent(col);
        }


        public void OnTriggerExit2D(Collider col)
        {
            if (onTriggerExitEvent != null)
                onTriggerExitEvent(col);
        }

        #endregion


        [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
        void DrawRay(Vector2 start, Vector2 dir, Color color)
        {
            Debug.DrawLine(start, dir, color);
        }


        #region Public

        /// <summary>
        /// attempts to move the character to position + deltaMovement. Any colliders in the way will cause the movement to
        /// stop when run into.
        /// </summary>
        /// <param name="deltaMovement">Delta movement.</param>
        public Vector2 Move(Vector2 deltaMovement)
        {
            // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
            collisionState.WasGroundedLastFrame = IsGrounded;

            // clear our state
            collisionState.Reset();
            _raycastHitsThisFrame.Clear();
            _isGoingUpSlope = false;
            _slopeHighestPoint = float.MaxValue;

            primeRaycastOrigins();

            // first, we check for a slope below us before moving
            // only check slopes if we are going down and grounded
            if(deltaMovement.Y > 0)
                HandleVerticalSlope(ref deltaMovement);

            // now we check movement in the horizontal dir
            if (deltaMovement.X != 0f)
                MoveHorizontally(ref deltaMovement);

            // next, check movement in the vertical dir
            if (deltaMovement.Y != 0f)
                MoveVertically(ref deltaMovement);

            
            // move then update our state
            //deltaMovement.z = 0;
            Entity.Transform.Position += deltaMovement;

            // only calculate velocity if we have a non-zero deltaTime
            if (Time.DeltaTime > 0f)
                velocity = deltaMovement / Time.DeltaTime;

            // set our becameGrounded state based on the previous and current collision state
            if (!collisionState.WasGroundedLastFrame && collisionState.Below)
                collisionState.BecameGroundedThisFrame = true;

            //// If we are grounded we set the down velocity to a small amount to stick on the floor
            //if (collisionState.Below)
            //    velocity.Y = 0.01f;

            // if we are going up a slope we artificially set a y velocity so we need to zero it out here
            if (_isGoingUpSlope)
                velocity.Y = 0;

            // send off the collision events if we have a listener
            if (onControllerCollidedEvent != null)
            {
                for (var i = 0; i < _raycastHitsThisFrame.Count; i++)
                    onControllerCollidedEvent(_raycastHitsThisFrame[i]);
            }

            ignoreOneWayPlatformsThisFrame = false;

            return deltaMovement;
        }


        /// <summary>
        /// moves directly down until grounded
        /// </summary>
        public void WarpToGrounded()
        {
            do
            {
                Move(Vector2.UnitY);
            } while (!IsGrounded);
        }


        /// <summary>
        /// this should be called anytime you have to modify the BoxCollider2D at runtime. It will recalculate the distance between the rays used for collision detection.
        /// It is also used in the skinWidth setter in case it is changed at runtime.
        /// </summary>
        public void recalculateDistanceBetweenRays()
        {
            // figure out the distance between our rays in both directions
            // horizontal
            var colliderUseableHeight = boxCollider.Bounds.Size.Y * Math.Abs(Entity.Transform.LocalScale.Y) - (2f * _skinWidth);
            _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

            // vertical
            var colliderUseableWidth = boxCollider.Bounds.Size.X * Math.Abs(Entity.Transform.LocalScale.X) - (2f * _skinWidth);
            _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
        }

        #endregion


        #region Movement Methods

        /// <summary>
        /// resets the raycastOrigins to the current extents of the box collider inset by the skinWidth. It is inset
        /// to avoid casting a ray from a position directly touching another collider which results in wonky normal data.
        /// </summary>
        /// <param name="futurePosition">Future position.</param>
        /// <param name="deltaMovement">Delta movement.</param>
        void primeRaycastOrigins()
        {
            // our raycasts need to be fired from the bounds inset by the skinWidth
            var modifiedBounds = boxCollider.Bounds;
            var amount = -2f * _skinWidth;
            modifiedBounds.Inflate(amount, amount);

            _raycastOrigins.topLeft = new Vector2(modifiedBounds.Left, modifiedBounds.Top);
            _raycastOrigins.bottomRight = new Vector2(modifiedBounds.Right, modifiedBounds.Bottom);
            _raycastOrigins.bottomLeft = new Vector2(modifiedBounds.Left, modifiedBounds.Bottom);
        }


        /// <summary>
        /// we have to use a bit of trickery in this one. The rays must be cast from a small distance inside of our
        /// collider (skinWidth) to avoid zero distance rays which will get the wrong normal. Because of this small offset
        /// we have to increase the ray distance skinWidth then remember to remove skinWidth from deltaMovement before
        /// actually moving the player
        /// </summary>
        void MoveHorizontally(ref Vector2 deltaMovement)
        {
            var isGoingRight = deltaMovement.X > 0;
            var rayDistance = Math.Abs(deltaMovement.X) + _skinWidth + checkPadding.X;
            var rayDirection = isGoingRight ? Vector2.UnitX : -Vector2.UnitX;
            var padding = isGoingRight ? -checkPadding.X : checkPadding.X;
            var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
            initialRayOrigin.X += padding;

            for (var i = 0; i < totalHorizontalRays; i++)
            {
                var ray = new Vector2(initialRayOrigin.X, initialRayOrigin.Y - i * _verticalDistanceBetweenRays);

                DrawRay(ray, ray + rayDirection * rayDistance, Color.Red);

                // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
                // walk up sloped oneWayPlatforms
                if (i == 0 && collisionState.WasGroundedLastFrame)
                    _raycastHit = Physics.Linecast(ray, ray + rayDirection * rayDistance, (int)platformMask);
                else
                    _raycastHit = Physics.Linecast(ray, ray + rayDirection * rayDistance, (int)(platformMask & ~oneWayPlatformMask));

                if (_raycastHit.Collider != null)
                {
                    //// the bottom ray can hit a slope but no other ray can so we have special handling for these cases
                    //if (i == 0 && handleHorizontalSlope(ref deltaMovement, Nez.Vector2Ext.Angle(_raycastHit.Normal, -Vector2.UnitY)))
                    //{
                    //    _raycastHitsThisFrame.Add(_raycastHit);
                    //    // if we weren't grounded last frame, that means we're landing on a slope horizontally.
                    //    // this ensures that we stay flush to that slope
                    //    if (!collisionState.WasGroundedLastFrame)
                    //    {
                    //        float flushDistance = Math.Sign(deltaMovement.X) * (_raycastHit.Distance - skinWidth);
                    //        Entity.Transform.Position += new Vector2(flushDistance, 0);
                    //    }
                    //    break;
                    //}

                    // We ignore slopes on horizontal collision
                     if (Nez.Vector2Ext.Angle(_raycastHit.Normal, -Vector2.UnitX) % 180 != 0)
                        continue;

                    if (_raycastHit.Point.Y > _slopeHighestPoint)
                        continue;

                    // set our new deltaMovement and recalculate the rayDistance taking it into account
                    deltaMovement.X = _raycastHit.Point.X - ray.X + padding;
                    rayDistance = Math.Abs(deltaMovement.X);

                    // remember to remove the skinWidth from our deltaMovement
                    if (isGoingRight)
                    {
                        deltaMovement.X -= _skinWidth;
                        collisionState.Right = true;
                    }
                    else
                    {
                        deltaMovement.X += _skinWidth;
                        collisionState.Left = true;
                    }

                    _raycastHitsThisFrame.Add(_raycastHit);

                    // we add a small fudge factor for the float operations here. if our rayDistance is smaller
                    // than the width + fudge bail out because we have a direct impact
                    if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                        break;
                }
            }
        }


        ///// <summary>
        ///// handles adjusting deltaMovement if we are going up a slope.
        ///// </summary>
        ///// <returns><c>true</c>, if horizontal slope was handled, <c>false</c> otherwise.</returns>
        ///// <param name="deltaMovement">Delta movement.</param>
        ///// <param name="angle">Angle.</param>
        //bool handleHorizontalSlope(ref Vector2 deltaMovement, float angle)
        //{
        //    // disregard 90 degree angles (walls)
        //    if (Mathf.RoundToInt(angle) == 90)
        //        return false;

        //    // if we can walk on slopes and our angle is small enough we need to move up
        //    if (angle < slopeLimit)
        //    {
        //        // we only need to adjust the deltaMovement if we are not jumping
        //        // TODO: this uses a magic number which isn't ideal! The alternative is to have the user pass in if there is a jump this frame
        //        if (deltaMovement.Y > jumpingThreshold)
        //        {
        //            // apply the slopeModifier to slow our movement up the slope
        //            //var slopeModifier = slopeSpeedMultiplier.Evaluate(angle);
        //            //deltaMovement.X *= slopeModifier;

        //            // we dont set collisions on the sides for this since a slope is not technically a side collision.
        //            // smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
        //            // to our new x location using our good friend Pythagoras
        //            deltaMovement.Y = Math.Abs((float)Math.Tan(angle * Mathf.Deg2Rad) * deltaMovement.X);
        //            var isGoingRight = deltaMovement.X > 0;

        //            // safety check. we fire a ray in the direction of movement just in case the diagonal we calculated above ends up
        //            // going through a wall. if the ray hits, we back off the horizontal movement to stay in bounds.
        //            var ray = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
        //            RaycastHit raycastHit;
        //            if (collisionState.WasGroundedLastFrame)
        //                raycastHit = Physics.Linecast(ray, ray + deltaMovement * 5, (int)platformMask);
        //            else
        //                raycastHit = Physics.Linecast(ray, ray + deltaMovement * 5, (int)(platformMask & ~oneWayPlatformMask));

        //            for (int i = 0; i < totalVerticalRays / 2; i++)
        //            {

        //            }
        //            DrawRay(ray, ray + deltaMovement * 5, Color.Green);
        //            Debug.DrawHollowBox(ray, 20, Color.Gray);
        //            Debug.DrawHollowBox(ray + deltaMovement, 20, Color.Black);

        //            if (raycastHit.Collider != null)
        //            {
        //                // we crossed an edge when using Pythagoras calculation, so we set the actual delta movement to the ray hit location
        //                deltaMovement = raycastHit.Point - ray;
        //                if (isGoingRight)
        //                    deltaMovement.X -= _skinWidth;
        //                else
        //                    deltaMovement.X += _skinWidth;
        //            }

        //            _isGoingUpSlope = true;
        //            collisionState.Below = true;
        //            collisionState.SlopeAngle = -angle;
        //        }
        //    }
        //    else // too steep. get out of here
        //    {
        //        deltaMovement.X = 0;
        //    }

        //    return true;
        //}


        void MoveVertically(ref Vector2 deltaMovement)
        {
            var isGoingUp = deltaMovement.Y < 0;
            var rayDistance = Math.Abs(deltaMovement.Y) + _skinWidth;
            var rayDirection = isGoingUp ? -Vector2.UnitY : Vector2.UnitY;
            var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

            // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
            initialRayOrigin.X += deltaMovement.X;

            // if we are moving up, we should ignore the layers in oneWayPlatformMask
            var mask = platformMask;
            if ((isGoingUp && !collisionState.WasGroundedLastFrame) || ignoreOneWayPlatformsThisFrame)
                mask &= ~oneWayPlatformMask;

            for (var i = 0; i < totalVerticalRays; i++)
            {
                var ray = new Vector2(initialRayOrigin.X + i * _horizontalDistanceBetweenRays, initialRayOrigin.Y);

                DrawRay(ray, ray + rayDirection * rayDistance, Color.Red);
                _raycastHit = Physics.Linecast(ray, ray + rayDirection * rayDistance, (int)mask);
                if (_raycastHit.Collider != null)
                {
                    // Ignore sloped collisions
                    if (Nez.Vector2Ext.Angle(_raycastHit.Normal, -Vector2.UnitY) % 180 != 0)
                        continue;

                    // set our new deltaMovement and recalculate the rayDistance taking it into account
                    deltaMovement.Y = _raycastHit.Point.Y - ray.Y;
                    rayDistance = Math.Abs(deltaMovement.Y);

                    // remember to remove the skinWidth from our deltaMovement
                    if (isGoingUp)
                    {
                        deltaMovement.Y += _skinWidth;
                        collisionState.Above = true;
                    }
                    else
                    {
                        deltaMovement.Y -= _skinWidth;
                        collisionState.Below = true;
                    }

                    _raycastHitsThisFrame.Add(_raycastHit);

                    // this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
                    // where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
                    if (!isGoingUp && deltaMovement.Y > 0.00001f)
                        _isGoingUpSlope = true;

                    // we add a small fudge factor for the float operations here. if our rayDistance is smaller
                    // than the width + fudge bail out because we have a direct impact
                    if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                        break;
                }
            }
        }


        ///// <summary>
        ///// checks the center point under the BoxCollider2D for a slope. If it finds one then the deltaMovement is adjusted so that
        ///// the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.
        ///// </summary>
        ///// <param name="deltaMovement">Delta movement.</param>
        //private void handleVerticalSlope(ref Vector2 deltaMovement)
        //{
        //    // slope check from the center of our collider
        //    var centerOfCollider = (_raycastOrigins.bottomLeft.X + _raycastOrigins.bottomRight.X) * 0.5f;
        //    var rayDirection = Vector2.UnitY;

        //    // the ray distance is based on our slopeLimit
        //    var slopeCheckRayDistance = _slopeLimitTangent * (_raycastOrigins.bottomRight.X - centerOfCollider);

        //    var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.bottomLeft.Y);
        //    DrawRay(slopeRay, slopeRay + rayDirection * slopeCheckRayDistance, Color.Yellow);
        //    _raycastHit = Physics.Linecast(slopeRay, slopeRay + rayDirection * slopeCheckRayDistance, (int)platformMask);
        //    if (_raycastHit.Collider != null)
        //    {
        //        // bail out if we have no slope
        //        var angle = Math.Abs(Nez.Vector2Ext.Angle(_raycastHit.Normal, -Vector2.UnitY));
        //        if (angle == 0)
        //            return;

        //        // we are moving down the slope if our normal and movement direction are in the same x direction
        //        var isMovingDownSlope = Math.Sign(_raycastHit.Normal.X) == Math.Sign(deltaMovement.X);
        //        if (isMovingDownSlope)
        //        {
        //            // going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
        //            //var slopeModifier = slopeSpeedMultiplier.Evaluate(-angle);
        //            // we add the extra downward movement here to ensure we "stick" to the surface below
        //            deltaMovement.Y += _raycastHit.Point.Y - slopeRay.Y - skinWidth;
        //            //deltaMovement = new Vector2(0, deltaMovement.Y) +
        //            //    new Vector2(deltaMovement.X * slopeModifier, 0).RotateX(-angle);
        //            collisionState.MovingDownSlope = true;
        //            collisionState.SlopeAngle = angle;
        //        }
        //    }
        //}

        /// <summary>
        /// checks the center point under the BoxCollider2D for a slope. If it finds one then the deltaMovement is adjusted so that
        /// the player stays grounded and the slopeSpeedModifier is taken into account to speed up movement.
        /// </summary>
        /// <param name="deltaMovement">Delta movement.</param>
        private void HandleVerticalSlope(ref Vector2 deltaMovement)
        {
            //var anchor = (_raycastOrigins.topLeft + _raycastOrigins.bottomRight) * 0.5f;
            var rayDirection = Vector2.UnitY;
            //var leastDistanceFromCenter = float.MaxValue;
            //var translation = 0f;
            //var rayDistance = boxCollider.Height * .5f - skinWidth;
            var padding = checkPadding.Y;
            var rayDistance = Math.Abs(deltaMovement.Y) + padding + EXTRA_SLOPE_RAY_DISTANCE + Math.Abs(deltaMovement.X) - SkinWidth;
            var rayOrigin = new Vector2((_raycastOrigins.bottomLeft.X + _raycastOrigins.bottomRight.X) / 2, _raycastOrigins.bottomLeft.Y - padding);

            DrawRay(rayOrigin, rayOrigin + rayDirection * rayDistance, Color.Yellow);
            _raycastHit = Physics.Linecast(rayOrigin, rayOrigin + rayDirection * rayDistance, (int)platformMask);

            if (_raycastHit.Collider == null)
                return;

            // bail out if we have no slope
            var angle = Math.Abs(Nez.Vector2Ext.Angle(_raycastHit.Normal, -Vector2.UnitY));
            if (angle == 0)
                return;

            _slopeHighestPoint = _raycastHit.Collider.Bounds.Top;

            deltaMovement.Y = _raycastHit.Point.Y - rayOrigin.Y - SkinWidth - padding;
            collisionState.Below = true;
            collisionState.SlopeAngle = angle;

            //for (int i = 0; i < totalVerticalRays; i++)
            //{
            //    var rayOrigin = new Vector2(_raycastOrigins.bottomLeft.X + i * _horizontalDistanceBetweenRays, anchor.Y);
            //    var distanceFromCenterX = Math.Abs(rayOrigin.X - anchor.X);
            //    float xDistancePercent = 1 - distanceFromCenterX * 0.5f / Math.Abs(_raycastOrigins.topLeft.X - anchor.X);
            //    var currentRayDistance = rayDistance * xDistancePercent;
            //    DrawRay(rayOrigin, rayOrigin + rayDirection * currentRayDistance, Color.Yellow);
            //    _raycastHit = Physics.Linecast(rayOrigin, rayOrigin + rayDirection * currentRayDistance, (int)platformMask);

            //    if (_raycastHit.Collider == null)
            //        continue;

            //    // bail out if we have no slope
            //    var angle = Math.Abs(Nez.Vector2Ext.Angle(_raycastHit.Normal, -Vector2.UnitY));
            //    if (angle == 0)
            //        continue;


            //    // We prioritize checking the center position so that we "sink" slightly into the slope
            //    if (distanceFromCenterX >= leastDistanceFromCenter)
            //        continue;

            //    leastDistanceFromCenter = distanceFromCenterX;
            //    translation = _raycastHit.Point.Y - rayOrigin.Y - currentRayDistance + skinWidth;
            //    collisionState.BecameGroundedThisFrame = true;
            //    collisionState.Below = true;
            //    collisionState.SlopeAngle = angle;
            //}

            //Entity.Transform.Position += Vector2.UnitY * translation;
            //deltaMovement.Y += translation;
        }

        #endregion

    }
}

