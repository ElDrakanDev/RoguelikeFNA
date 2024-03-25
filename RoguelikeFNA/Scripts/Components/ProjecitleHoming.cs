using Nez;
using Nez.ImGuiTools.ObjectInspectors;
using Microsoft.Xna.Framework;
using System.Linq;
using RoguelikeFNA.Prefabs;

namespace RoguelikeFNA
{
    public class ProjecitleHoming : Component, IUpdatable
    {
        Projectile _proj;
        [Inspectable] float _attractSpeed = 0.1f;
        [Inspectable] float _homingRange = 150f;

        public override void OnAddedToEntity()
        {
            _proj = Entity.GetComponent<Projectile>();
        }

        public void Update()
        {
            var closest = Core.Scene.Entities.FindComponentsOfType<EntityStats>()
                .Where((stats) => Flags.IsFlagSet(_proj.TargetTeams, (int)stats.Team)
                    && stats.InRange(Transform.Position, _homingRange)
                    && (_proj.GroundHitBehaviour == GroundHitBehaviour.Ignore || stats.LineOfSight(Transform.Position)))
                .ToArray().Closest(Transform.Position);
            if(closest != null)
                _proj.Velocity += (closest.Transform.Position - Transform.Position) * Time.DeltaTime * _attractSpeed;
        }

        [InspectorDelegate]
        public void DebugDraw()
        {
            Debug.DrawCircle(Transform.Position, Color.Red, _homingRange);
        }
    }
}
