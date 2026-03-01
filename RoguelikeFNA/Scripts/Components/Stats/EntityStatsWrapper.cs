using System;
using System.Collections.Generic;
using Nez;

namespace RoguelikeFNA.Utils
{
    [Serializable]
    public class EntityStatsWrapper : Component
    {
        const float DEFAULT_MIN_STAT = 0.1f;
        public Dictionary<StatID, float> BaseStats = new();
        public EntityTeam Team;
        public EntityTeam TargetTeams;

        public static Dictionary<StatID, float> DefaultStats = new()
        {
            {StatID.Damage, 1},
            {StatID.Health, 25}
        };

        public override void OnAddedToEntity()
        {
            // Add any missing stats from DefaultStats to BaseStats
            foreach (var defaultStat in DefaultStats)
            {
                if (!BaseStats.ContainsKey(defaultStat.Key))
                {
                    BaseStats[defaultStat.Key] = defaultStat.Value;
                }
            }

            var stats = new EntityStats
            {
                Team = Team,
                TargetTeams = (int)TargetTeams
            };
            foreach (var stat in BaseStats)
            {
                var min = DEFAULT_MIN_STAT;
                if (stat.Key == StatID.Health)
                    min = 1;
                stats.Stats[stat.Key] = new Stat(stat.Value, Entity, min);
            }
            Entity.AddComponent(stats);
        }
    }
}