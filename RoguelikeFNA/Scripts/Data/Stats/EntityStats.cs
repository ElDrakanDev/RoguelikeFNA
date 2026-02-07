using Nez;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    [System.Flags]
    public enum EntityTeam
    {
        Friendly = 1,
        Neutral = 2,
        Enemy = 4
    }
    public enum StatID
    {
        Damage,
        Health,
    }
    public class EntityStats : Component
    {
        const float DEFAULT_MIN_STAT = 0.1f;
        public readonly Dictionary<StatID, Stat> Stats = new();
        [Inspectable] public EntityTeam Team;
        [Inspectable] public int TargetTeams;
        public HealthController HealthManager { get; private set; }

        public EntityStats(float damage = 1)
        {
            Stats[StatID.Damage] = new Stat(damage, null, DEFAULT_MIN_STAT);
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            HealthManager = Entity.GetComponent<HealthController>();
            Stats[StatID.Health] = HealthManager.MaxHealth;

            foreach (var stat in Stats.Values)
                stat.Owner = Entity;

            // If no team is set we target any team that isnt our own
            if (TargetTeams == 0)
            {
                TargetTeams = (int)Team;
                Flags.InvertFlags(ref TargetTeams);
            }
        }

        public Stat this[StatID id] => Stats[id];

        public Stat[] AllStats() => Stats.Values.ToArray();
    }
}
