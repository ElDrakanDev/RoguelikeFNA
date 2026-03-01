using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    [Flags]
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
        Iframes,
    }
    public class EntityStats : Component
    {
        public readonly Dictionary<StatID, Stat> Stats = new();
        [Inspectable] public EntityTeam Team;
        [Inspectable] public int TargetTeams;
        public StatBool IsInvincible = new() { Type = StatBool.ConditionType.All };
        public StatBool IsIntangible = new() { Type = StatBool.ConditionType.Any };

        public EntityStats() { }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

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

        public bool TryGetStat(StatID id, out Stat stat) => Stats.TryGetValue(id, out stat);
    }
}
