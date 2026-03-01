using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using RoguelikeFNA.Generation;
using RoguelikeFNA.Utils;

namespace RoguelikeFNA.Entities
{
    [Serializable]
    public class RangedEnemyAI : Component, IUpdatable
    {
        [Inspectable] public float MoveSpeed = 70f;
        [Inspectable] public float DetectionRadius = 300f;
        [Inspectable] public float ShootRange = 150f;
        [Inspectable] public float ShootCooldown = 2f;
        [Inspectable] public float ProjectileSpeed = 100f;
        [Inspectable] public float PathRecalculateEvery = 0.25f;
        [Inspectable] public float WaypointReachDistance = 8f;

        PhysicsBody _body;
        FaceDirection _faceDirection;
        EntityStats _stats;
        SerializedEntity _projectilePrefab;
        LevelNavigator _levelNavigator;
        GameEntity _self;

        readonly List<Vector2> _currentPath = new();
        int _pathIndex;
        float _shootTimer;
        float _repathTimer;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _self = Entity.GetComponent<GameEntity>();
            _body = Entity.GetComponent<PhysicsBody>();
            _faceDirection = Entity.GetComponent<FaceDirection>();
            _stats = Entity.GetComponent<EntityStats>();
            _levelNavigator = Entity.Scene.GetSceneComponent<LevelNavigator>();
            _projectilePrefab = Entity.Scene.Content.LoadNson<SerializedEntity>(ContentPath.Serializables.Entities.Bullet_nson);
            _shootTimer = 0f;
            _repathTimer = 0f;
        }

        public void Update()
        {
            if (_stats is null || _body is null)
                return;

            _shootTimer -= Time.DeltaTime;
            _repathTimer -= Time.DeltaTime;

            var target = GetClosestTargetInDetectionRadius();
            if (target is null)
            {
                _currentPath.Clear();
                _body.Velocity = Vector2.Zero;
                return;
            }

            var toTarget = target.Transform.Position - Transform.Position;
            var distanceToTarget = toTarget.Length();
            var hasLineOfSight = target.LineOfSight(Transform.Position);

            if (hasLineOfSight)
            {
                _currentPath.Clear();

                if (distanceToTarget <= ShootRange)
                {
                    _body.Velocity = Vector2.Zero;
                    TryShootAt(target);
                }
                else
                {
                    var direction = SafeNormalize(toTarget);
                    _body.Velocity = direction * MoveSpeed;
                    _faceDirection?.CheckFacingSide(direction.X);
                }

                return;
            }

            FollowPathTowards(target.Transform.Position);
        }

        GameEntity GetClosestTargetInDetectionRadius()
        {
            return GameEntityManager.Entities
                .OfTeam(_stats.TargetTeams)
                .Alive()
                .Enabled()
                .Excluding(_self)
                .InRange(Transform.Position, DetectionRadius)
                .ClosestTo(Transform.Position);
        }

        void FollowPathTowards(Vector2 targetPosition)
        {
            if (_levelNavigator?.CurrentRoom is null)
            {
                _body.Velocity = Vector2.Zero;
                return;
            }

            if (_repathTimer <= 0f || _currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
            {
                RebuildPath(targetPosition);
            }

            if (_currentPath.Count == 0 || _pathIndex >= _currentPath.Count)
            {
                _body.Velocity = Vector2.Zero;
                return;
            }

            var waypoint = _currentPath[_pathIndex];
            var toWaypoint = waypoint - Transform.Position;
            var distance = toWaypoint.Length();

            if (distance <= WaypointReachDistance)
            {
                _pathIndex++;
                if (_pathIndex >= _currentPath.Count)
                {
                    _body.Velocity = Vector2.Zero;
                    return;
                }

                waypoint = _currentPath[_pathIndex];
                toWaypoint = waypoint - Transform.Position;
            }

            var direction = SafeNormalize(toWaypoint);
            _body.Velocity = direction * MoveSpeed;
            _faceDirection?.CheckFacingSide(direction.X);
        }

        void RebuildPath(Vector2 targetPosition)
        {
            _repathTimer = PathRecalculateEvery;
            _currentPath.Clear();
            _pathIndex = 0;

            var path = _levelNavigator.CurrentRoom.FindPath(Transform.Position, targetPosition);
            if (path is null || path.Count == 0)
                return;

            _currentPath.AddRange(path);
            if (_currentPath.Count > 1)
                _pathIndex = 1;
        }

        void TryShootAt(GameEntity target)
        {
            if (_shootTimer > 0f || _projectilePrefab is null)
                return;

            _shootTimer = ShootCooldown;

            var direction = SafeNormalize(target.Transform.Position - Transform.Position);
            if (direction == Vector2.Zero)
                direction = _faceDirection?.FacingRight == false ? -Vector2.UnitX : Vector2.UnitX;

            var projectileEntity = _projectilePrefab.AddToScene(Entity.Scene)
                .SetPosition(Transform.Position)
                .SetLocalScale(Transform.LocalScale)
                .SetParent(Entity.Parent);

            projectileEntity.GetComponent<PhysicsBody>().Velocity = direction * ProjectileSpeed;
            projectileEntity.GetComponent<Projectile>()?.SetValuesFromEntityStats(_stats);
        }

        static Vector2 SafeNormalize(Vector2 input)
        {
            if (input.LengthSquared() <= 0.0001f)
                return Vector2.Zero;

            input.Normalize();
            return input;
        }
    }
}
