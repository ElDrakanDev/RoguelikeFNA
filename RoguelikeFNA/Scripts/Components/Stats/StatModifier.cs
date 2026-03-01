using Nez;
using System;

namespace RoguelikeFNA
{
    public enum StatType
    {
        Flat = 10,
        Mult = 20
    }

    public interface IStatModifier
    {
        public float Value { get; set; }
        public StatType Type { get; set; }
        public object Source { get; set; }
        public bool IsDirty { get; set; }
    }

    [Serializable]
    public class StatModifier : IStatModifier
    {
        [Inspectable] float _value;

        [NotInspectable] public bool IsDirty { get; set; } = true;
        public object Source { get; set; }
        [Inspectable] public StatType Type { get; set; }
        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                IsDirty = true;
            }
        }

        public StatModifier(float value, object source, StatType type)
        {
            _value = value;
            Source = source;
            Type = type;
        }
    }

    public class RefStatModifier : IStatModifier
    {
        public Stat StatRef;
        public bool IsDirty { get => true; set { } }
        public object Source { get; set; }
        public StatType Type { get; set; }
        public float Ratio = 1f;
        public float Value
        {
            get { return StatRef.Value * Ratio; }
            set { }
        }

        public RefStatModifier(Stat statRef, object source, StatType type, float ratio = 1)
        {
            StatRef = statRef;
            Ratio = ratio;
            Source = source;
            Type = type;
        }
    }
}