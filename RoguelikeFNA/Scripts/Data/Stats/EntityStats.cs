using Nez;

namespace RoguelikeFNA
{
    public class EntityStats : Component
    {
        const float DEFAULT_MIN_STAT = 0.1f;
        public readonly Stat Damage;

        public EntityStats(float damage = 1)
        {
            Damage = new Stat(damage, null, DEFAULT_MIN_STAT);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            Damage.Owner = Entity;
        }
    }
}
