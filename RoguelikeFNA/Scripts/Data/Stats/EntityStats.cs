using Nez;

namespace RoguelikeFNA
{
    [System.Flags]
    public enum EntityTeam
    {
        Friendly = 1,
        Neutral = 2,
        Enemy = 4
    }
    public class EntityStats : Component
    {
        const float DEFAULT_MIN_STAT = 0.1f;
        public readonly Stat Damage;
        public EntityTeam Team;
        public int TargetTeams;
        public HealthManager HealthManager { get; private set; }

        public EntityStats(float damage = 1)
        {
            Damage = new Stat(damage, null, DEFAULT_MIN_STAT);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            HealthManager = Entity.GetComponent<HealthManager>();
            Damage.Owner = Entity;
            // If no team is set we target any team that isnt our own
            if(TargetTeams == 0)
            {
                TargetTeams = (int)Team;
                Flags.InvertFlags(ref TargetTeams);
            }
        }
    }
}
