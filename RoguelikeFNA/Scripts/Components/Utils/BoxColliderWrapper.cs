using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA.Utils
{
    public class BoxColliderWrapper : Component
    {
        [Inspectable] public float Width;
        [Inspectable] public float Height;
        [Inspectable] public Vector2 Offset;
        [Inspectable] public CollisionLayer PhysicsLayer = CollisionLayer.None;
        [Inspectable] public CollisionLayer CollidesWithLayers = CollisionLayer.None;

        public override void Initialize()
        {
            if(Entity.HasComponent<BoxCollider>())
                return;
            var boxCollider = new BoxCollider(Width, Height);
            boxCollider.LocalOffset = Offset;
            boxCollider.PhysicsLayer = (int)PhysicsLayer;
            boxCollider.CollidesWithLayers = (int)CollidesWithLayers;
            Entity.AddComponent(boxCollider);
        }

        public override Component Clone()
        {
            return base.Clone();
        }
    }
}