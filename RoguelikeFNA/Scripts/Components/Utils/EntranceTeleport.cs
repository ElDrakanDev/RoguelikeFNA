using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using RoguelikeFNA.Generation;

namespace RoguelikeFNA.Utils
{
    [Serializable]
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

        void MoveToEntrance(Room room)
        {
            var entrances = Entity.Scene.FindEntitiesWithTag((int)Tag.Entrance).Enabled();
            if (entrances.Count() > 0)
            {
                Entity.Position = entrances.First().Position;
                if(Entity.TryGetComponent(out PhysicsBody body))
                    body.Velocity = Vector2.Zero;
            }
        }
    }
}