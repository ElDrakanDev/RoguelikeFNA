using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    [Serializable]
    public class StatBool
    {
        public bool DefaultValue;
        public class Condition
        {
            public bool Value;
        }
        public enum ConditionType
        {
            Any, All
        }
        public readonly List<Condition> Conditions = new List<Condition>();
        public ConditionType Type;

        public bool Value
        {
            get
            {
                if(Conditions.Count == 0)
                    return DefaultValue;
                if (Type == ConditionType.Any)
                    return Conditions.Any(c => c.Value);
                else if (Type == ConditionType.All)
                    return Conditions.All(c => c.Value);
                return false;
            }
        }

        public static implicit operator bool(StatBool stat) => stat.Value;
    }

    [Serializable]
    public class Stat
    {
        /// <summary>
        /// Forces the stat to update the value on the next value get
        /// </summary>
        public readonly float Min;
        public readonly float Max;
        public Entity Owner;
        [Inspectable] readonly List<IStatModifier> _stats = new List<IStatModifier>();

        public float Value => CalculateValue();

        public Stat(float baseValue, Entity owner, float min = 1, float max = float.MaxValue)
        {
            _stats.Add(new StatModifier(baseValue, this, StatType.Flat));
            Min = min;
            Max = max;
            Owner = owner;
        }

        private float CalculateValue()
        {
            float flatStats = 0;
            float multStats = 1;

            foreach(var stat in _stats)
            {
                if (stat.Type == StatType.Flat)
                    flatStats += stat.Value;
                else if (stat.Type == StatType.Mult)
                    multStats += stat.Value;
                stat.IsDirty = false;
            }

            return (float)Math.Round(flatStats * multStats, 2);
        }

        public void Add(IStatModifier modifier)
        {
            _stats.Add(modifier);
        }

        public bool RemoveModifier(IStatModifier modifier)
        {
            return _stats.Remove(modifier);
        }
        
        public void RemoveFromSource(object source)
        {
            _stats.RemoveAll((stat) =>
            {
                bool remove = stat.Source == source;
                return remove;
            });
        }

        public static implicit operator float(Stat stat) => stat.Value;
    }
}
