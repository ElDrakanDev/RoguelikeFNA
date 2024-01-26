using Nez;
using System;
using System.Collections.Generic;

namespace RoguelikeFNA
{
    [Serializable]
    public class Stat
    {
        #region Variables and constructor
        /// <summary>
        /// Forces the stat to update the value on the next value get
        /// </summary>
        public bool NeedUpdate = true;
        public readonly float Min;
        public readonly float Max;
        public Entity Owner;
        public float Value
        {
            get
            {
                if (!NeedUpdate)
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
        private float _value;
        [Inspectable] readonly List<StatModifier> _stats = new List<StatModifier>();
        public Stat(float baseValue, Entity owner, float min = 1, float max = float.MaxValue)
        {
            _stats.Add(new StatModifier(baseValue, this, this, StatType.Flat));
            Min = min;
            Max = max;
            Owner = owner;
        }
        #endregion
        #region Modifiers Management
        private float UpdateValue()
        {
            float flatStats = 0;
            float multStats = 1;

            for (int i = 0; i < _stats.Count; i++)
            {
                if (_stats[i].Type == StatType.Flat)
                    flatStats += _stats[i].Value;
                else if (_stats[i].Type == StatType.Mult)
                    multStats += _stats[i].Value;
            }

            NeedUpdate = false;
            _value = (float)Math.Round(flatStats * multStats, 2);
            return _value;
        }
        public void Add(StatModifier modifier)
        {
            _stats.Add(modifier);
            NeedUpdate = true;
        }
        public bool RemoveModifier(StatModifier modifier)
        {
            NeedUpdate = true;
            return _stats.Remove(modifier);
        }
        public void RemoveFromSource(object source)
        {
            _stats.RemoveAll((stat) => {
                bool remove = stat.Source == source;
                return remove;
            });
            NeedUpdate = true;
        }
        #endregion
        public static implicit operator float(Stat stat) => stat.Value;
    }
}
