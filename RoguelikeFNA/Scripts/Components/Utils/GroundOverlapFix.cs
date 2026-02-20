using Nez;

namespace RoguelikeFNA.Utils
{
    public class GroundOverlapFix : Component
    {
        public override void OnEnabled()
        {
            AdjustPositionToPreventGroundOverlap();
        }

        void AdjustPositionToPreventGroundOverlap()
        {
            var col = Entity.GetComponent<Collider>();
            // Adjust position to prevent overlapping with ground
            if (col == null || col.IsTrigger)
                return;

            var broadphase = Physics.BoxcastBroadphase(col.Bounds, (int)CollisionLayer.Ground);
            
            foreach (var collider in broadphase)
            {
                if (collider.IsTrigger)
                    continue;

                if (col.CollidesWith(collider, out var result))
                {
                    // Apply position adjustment to separate from ground
                    Transform.Position -= result.MinimumTranslationVector;
                }
            }
            
        }
    }
}