using Nez;

namespace RoguelikeFNA
{
    public class EssenceOnDeath : Component
    {
        public int EssenceDropped;

        public EssenceOnDeath(int essence)
        {
            EssenceDropped = essence;
        }

        public override void OnAddedToEntity()
        {
            Entity.GetComponent<HealthController>().onDeath += DropEssence;
        }

        public override void OnRemovedFromEntity()
        {
            Entity.GetComponent<HealthController>().onDeath -= DropEssence;
        }

        void DropEssence(DeathInfo dInfo)
        {
            Essence.Create(Entity.Position, EssenceDropped);
        }
    }
}