using System;
using Nez;

namespace RoguelikeFNA
{
    [Serializable]
    public class EssenceOnDeath : Component, IDeathListener
    {
        public int EssenceDropped;

        public override void OnAddedToEntity()
        {
            Entity.GetComponent<HealthController>().DeathListeners.Add(this);
        }

        public override void OnRemovedFromEntity()
        {
            Entity.GetComponent<HealthController>().DeathListeners.Remove(this);
        }

        public void OnDeath(DeathInfo deathInfo)
        {
            Essence.Create(Entity.Position, EssenceDropped);
        }
    }
}