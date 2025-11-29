using System.Collections.Generic;
using System.Linq;
using Nez;
using Microsoft.Xna.Framework;

namespace RoguelikeFNA
{
    public class CompositeCollider : Collider
    {
        Entity _colliderContainer;
        List<Collider> _colliders = new();
        HashSet<Collider> _collidersSet => new();

        public new bool IsTrigger
        {
            get => base.IsTrigger;
            set
            {
                base.IsTrigger = value;
                foreach (var collider in _colliders)
                    collider.IsTrigger = value;
            }
        }

        public new int PhysicsLayer
        {
            get => base.PhysicsLayer;
            set
            {
                base.PhysicsLayer = value;
                foreach (var collider in _colliders)
                    collider.PhysicsLayer = value;
            }
        }

        public new int CollidesWithLayers
        {
            get => base.CollidesWithLayers;
            set
            {
                base.CollidesWithLayers = value;
                foreach (var collider in _colliders)
                    collider.CollidesWithLayers = value;
            }
        }
        
        bool _boundsDirty = true;
        RectangleF _bounds;
        public override RectangleF Bounds => _boundsDirty ? CalculateBoundsAndGet() : _bounds;

        void CalculateBounds()
        {
            if (_colliders.Count == 0)
            {
                _bounds = new RectangleF();
                return;
            }

            var firstBounds = _colliders[0].Bounds;
            float left = firstBounds.Left;
            float right = firstBounds.Right;
            float top = firstBounds.Top;
            float bottom = firstBounds.Bottom;

            for (int i = 1; i < _colliders.Count; i++)
            {
                var b = _colliders[i].Bounds;
                if (b.Left < left) left = b.Left;
                if (b.Right > right) right = b.Right;
                if (b.Top < top) top = b.Top;
                if (b.Bottom > bottom) bottom = b.Bottom;
            }

            _bounds = new RectangleF(left, top, right - left, bottom - top);
        }

        RectangleF CalculateBoundsAndGet()
        {
            CalculateBounds();
            _boundsDirty = false;
            return _bounds;
        }

        public new void SetLocalOffset(Vector2 localOffset)
        {
            _colliderContainer.SetLocalPosition(localOffset);
            _boundsDirty = true;
        }

        public void AddCollider(Collider collider, Vector2 offset = default)
        {
            _colliders.Add(collider);
            _collidersSet.Add(collider);
            collider.SetLocalOffset(offset);
            if(_colliderContainer != null)
                _colliderContainer.AddComponent(collider);
        }

        public void RemoveCollider(Collider collider)
        {
            if (_collidersSet.Remove(collider))
            {
                _colliders.Remove(collider);
                _colliderContainer.RemoveComponent(collider);
            }
        }

        public override Component Clone()
        {
            CompositeCollider clone = (CompositeCollider)base.Clone();
            clone._colliders = _colliders.Select(c => (Collider)c.Clone()).ToList();

            return clone;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _colliderContainer = Entity.Scene.CreateEntity("CompositeContainer");
            _colliderContainer.SetParent(Entity);
            foreach (var collider in _colliders)
                _colliderContainer.AddComponent(collider);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();
            _colliderContainer.Destroy();
        }

        public override void RegisterColliderWithPhysicsSystem()
        {
            foreach (var collider in _colliders)
                collider.RegisterColliderWithPhysicsSystem();
        }

        public override void UnregisterColliderWithPhysicsSystem()
        {
            foreach (var collider in _colliders)
                collider.UnregisterColliderWithPhysicsSystem();
        }

        public new bool Overlaps(Collider collider)
        {
            foreach (var childCollider in _colliders)
            {
                if (childCollider.Overlaps(collider))
                    return true;
            }
            return false;
        }

        public new bool CollidesWith(Collider collider, out CollisionResult collisionResult)
        {
            foreach (var childCollider in _colliders)
            {
                if (childCollider.CollidesWith(collider, out collisionResult))
                    return true;
            }
            collisionResult = default;
            return false;
        }

        public new bool CollidesWith(Collider collider, Vector2 motion, out CollisionResult result)
        {
            foreach (var childCollider in _colliders)
            {
                if (childCollider.CollidesWith(collider, motion, out result))
                    return true;
            }
            result = default;
            return false;
        }

        public new bool CollidesWithAny(ref Vector2 motion, out CollisionResult result)
        {
			// fetch anything that we might collide with at our new position
			var colliderBounds = Bounds;
			colliderBounds.X += motion.X;
			colliderBounds.Y += motion.Y;
			var neighbors = Physics.BoxcastBroadphase(ref colliderBounds, _collidersSet, CollidesWithLayers);

			foreach (var neighbor in neighbors)
			{
				// skip triggers
				if (neighbor.IsTrigger)
					continue;

				if (CollidesWith(neighbor, motion, out result))
				{
					return true;
				}
			}

            result = default;
			return false;
        }
    }
}