using System;
using System.Collections.Generic;
using Nez;

namespace RoguelikeFNA.Utils
{
    [Serializable]
    public class EntityStatsWrapper : Component
    {
        const float DEFAULT_MIN_STAT = -0.9f;
        public Dictionary<StatID, float> BaseStats = new();
        public EntityTeam Team;
        public EntityTeam TargetTeams;

        public static Dictionary<StatID, float> DefaultStats = new()
        {
            {StatID.Damage, 0},
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
            foreach (var baseStat in BaseStats)
            {
                var min = DEFAULT_MIN_STAT;
                if (baseStat.Key == StatID.Health)
                    min = 1;
                var stat = new Stat(baseStat.Value, Entity, min);
                stats.Stats[baseStat.Key] = stat;
            }
            Entity.AddComponent(stats);
        }
    }
}