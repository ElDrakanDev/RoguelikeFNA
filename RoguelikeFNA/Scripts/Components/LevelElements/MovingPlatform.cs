using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace RoguelikeFNA.LevelElements
{
    [Serializable]
    public class PointSequence : Component
    {
        public TiledEntity NextPoint;
    }

    [Serializable]
    public class MovingPlatform : Component, IUpdatable
    {
        public float MoveSpeed = 50f;
        public Vector2 DeltaMovement {get; private set;}
        public TiledEntity Start;
        TiledEntity[] _points;
        int _currentPointIndex = 0;

        // public override void OnAddedToEntity()
        // {
        //     var entity = Entity as TiledEntity;
        //     Entity.AddComponent(
        //         new BoxCollider(entity.Width, entity.Height)
        //         {
        //             CollidesWithLayers = (int)CollisionLayer.Entity,
        //             PhysicsLayer = (int)CollisionLayer.Platform
        //         }
        //     );
        // }

        public override void OnAddedToEntity()
        {
            var points = new List<TiledEntity>(){Start};
            var currPoint = GetNextPoint(Start);
            while(currPoint != null && currPoint != Start)
            {
                points.Add(currPoint);
                currPoint = GetNextPoint(currPoint);
            }
            _points = points.ToArray();
        }

        TiledEntity GetNextPoint(TiledEntity current)
        {
            if(current is null)
            {
                Debug.Warn("MovingPlatform {0} has a null point in its sequence.", Entity.Name);
                return null;
            }
            var point = current.GetComponent<PointSequence>();
            if(point is null)
            {
                Debug.Warn("MovingPlatform {0} has a point without a PointSequence component.", Entity.Name);
                return null;
            }
            return point.NextPoint;
        }

        public void Update()
        {
            if (_points == null || _points.Length == 0)
                return;

            var targetPoint = _points[_currentPointIndex];
            var direction = targetPoint.Position - Entity.Position;
            var distance = direction.Length();

            // If we're close enough to the target point, move to the next one
            if (distance < 1f)
            {
                _currentPointIndex = (_currentPointIndex + 1) % _points.Length;
                return;
            }

            // Move towards the target point
            direction.Normalize();
            DeltaMovement = direction * MoveSpeed * Time.DeltaTime;
            Entity.Position += DeltaMovement;
        }
    }
}