using System;
using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA.Utils
{
    [Serializable]
    public class BoxColliderWrapper : Component
    {
        [Inspectable] public float Width;
        [Inspectable] public float Height;
        [Inspectable] public Vector2 Offset;
        [Inspectable] public CollisionLayer PhysicsLayer = CollisionLayer.None;
        [Inspectable] public CollisionLayer CollidesWithLayers = CollisionLayer.None;
        [Inspectable] public bool IsTrigger = false;
        public bool UseTiledSize = false;

        public override void Initialize()
        {
            if(Entity.HasComponent<BoxCollider>())
                return;
            float width = Width;
            float height = Height;

            if (UseTiledSize && Entity is TiledEntity tiledEntity)
            {
                width = tiledEntity.Width;
                height = tiledEntity.Height;
            }

            var boxCollider = new BoxCollider(width, height)
            {
                LocalOffset = Offset,
                PhysicsLayer = (int)PhysicsLayer,
                CollidesWithLayers = (int)CollidesWithLayers,
                IsTrigger = IsTrigger
            };
            Entity.AddComponent(boxCollider);
        }

        public override Component Clone()
        {
            return base.Clone();
        }
    }
}