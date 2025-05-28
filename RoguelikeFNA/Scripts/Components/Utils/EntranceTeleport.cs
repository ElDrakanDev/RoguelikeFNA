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
            var entrance = Entity.Scene.FindEntitiesWithTag((int)Tag.Entrance).Enabled().FirstOrDefault();
            if (entrance != null)
            {
                Entity.Position = entrance.Position;
            }
        }
    }
}