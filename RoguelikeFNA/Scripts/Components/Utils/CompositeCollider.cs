using System.Collections.Generic;
using System.Linq;
using Nez;

namespace RoguelikeFNA
{
    public class CompositeCollider : Collider
    {
        Entity _colliderContainer;
        public List<Collider> Colliders;
        
        public override RectangleF Bounds => base.Bounds;

        public override Component Clone()
        {
            CompositeCollider clone = (CompositeCollider)base.Clone();
            clone.Colliders = Colliders.Select(c => (Collider)c.Clone()).ToList();

            return clone;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _colliderContainer = Entity.Scene.CreateEntity("CompositeContainer");
            _colliderContainer.SetParent(Entity);
            foreach (var collider in Colliders)
                _colliderContainer.AddComponent(collider);
        }

        public override void RegisterColliderWithPhysicsSystem()
        {
            foreach (var collider in Colliders)
                collider.RegisterColliderWithPhysicsSystem();
        }

        public override void UnregisterColliderWithPhysicsSystem()
        {
            foreach (var collider in Colliders)
                collider.UnregisterColliderWithPhysicsSystem();
        }

    }
}