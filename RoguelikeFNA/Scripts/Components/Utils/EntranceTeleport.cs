using System.Linq;
using Nez;

namespace RoguelikeFNA
{
    public class EntranceTeleport : Component
    {
        public override void OnAddedToEntity()
        {
            MoveToEntrance(null);
        }

        public override void OnEnabled()
        {
            Entity.Scene.GetSceneComponent<LevelNavigator>().OnRoomChanged += MoveToEntrance;
        }

        public override void OnDisabled()
        {
            Entity.Scene.GetSceneComponent<LevelNavigator>().OnRoomChanged -= MoveToEntrance;
        }

        void MoveToEntrance(Entity tilemap)
        {
            var entrances = Entity.Scene.FindEntitiesWithTag((int)Tag.Entrance).Enabled();
            if (entrances.Count() > 0)
            {
                Entity.Position = entrances.First().Position;
            }
        }
    }
}