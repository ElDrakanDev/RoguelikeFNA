using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeFNA
{
    [Serializable]
    public class Stat
    {
        /// <summary>
        /// Forces the stat to update the value on the next value get
        /// </summary>
        public readonly float Min;
        public readonly float Max;
        public Entity Owner;
        private float _value;
        [Inspectable] readonly List<IStatModifier> _stats = new List<IStatModifier>();

        public float Value
        {
            get
            {
                if (!NeedsUpdate())
                {
                    if (_value < Min) return Min;
                    else if (_value > Max) return Max;
                    return _value;
                }
                UpdateValue();
                return _value;
            }
            set
            {
                if (value < Min) _value = Min;
                else if (value > Max) _value = Max;
                else _value = value;
            }
        }

        public Stat(float baseValue, Entity owner, float min = 1, float max = float.MaxValue)
        {
            _stats.Add(new StatModifier(baseValue, this, StatType.Flat));
            Min = min;
            Max = max;
            Owner = owner;
        }

        private float UpdateValue()
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

            _value = (float)Math.Round(flatStats * multStats, 2);
            return _value;
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

        public bool NeedsUpdate() => _stats.Any(s => s.IsDirty);

        public static implicit operator float(Stat stat) => stat.Value;
    }
}
