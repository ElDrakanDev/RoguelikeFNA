using Nez;
using System;

namespace RoguelikeFNA
{
    public enum StatType
    {
        Flat = 10,
        Mult = 20
    }
    [Serializable]
    public struct StatModifier
    {
        float _value;
        public StatType Type;
        [NotInspectable] public Stat StatOwner { get; private set; }
        public object Source;

        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (StatOwner != null)
                    StatOwner.NeedUpdate = true;
            }
        }

        public StatModifier(float value, object source, Stat owner, StatType type)
        {
            _value = value;
            Source = source;
            StatOwner = owner;
            Type = type;
        }
    }
}