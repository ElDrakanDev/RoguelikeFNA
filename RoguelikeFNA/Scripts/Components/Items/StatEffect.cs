using Nez;

namespace RoguelikeFNA.Items
{
    [System.Serializable]
    public class StatEffect : ItemEffect
    {
        public override string DescriptionId => "stat_effect_desc";
        [Inspectable] public StatType Type;
        [Inspectable] public float Value;
        [Inspectable] public StatID StatID;
        StatModifier _modifier;

        string StatSymbol => Type == StatType.Flat ? "" : "%";

        public override string GetDescription()
        {
            return string.Format(base.GetDescription(), StatID, Value, StatSymbol);
        }

        public override void OnPickup(Entity source, Item item)
        {
            base.OnPickup(source, item);
            _modifier = new StatModifier(Value, item, Type);
            var stats = source.GetComponent<EntityStats>();
            stats[StatID].Add(_modifier);
        }

        public override void OnRemove()
        {
            var stats = Owner.GetComponent<EntityStats>();
            stats[StatID].RemoveModifier(_modifier);
            base.OnRemove();
        }
    }

    [System.Serializable]
    public class RefStatEffect : ItemEffect
    {
        public override string DescriptionId => "refstat_effect_desc";
        RefStatModifier Modifier;
        [Inspectable] public StatID RefStatID;
        [Inspectable] public StatID StatID;
        [Inspectable] public float Ratio;
        [Inspectable] public StatType Type;
        string StatSymbol => "%";

        public override string GetDescription()
        {
            return string.Format(base.GetDescription(), StatID, Ratio, StatSymbol, RefStatID);
        }

        public override void OnPickup(Entity source, Item item)
        {
            base.OnPickup(source, item);

            var stats = source.GetComponent<EntityStats>();
            Modifier = new RefStatModifier(stats[RefStatID], item, Type, Ratio);
            stats[StatID].Add(Modifier);
        }

        public override void OnRemove()
        {
            var stats = Owner.GetComponent<EntityStats>();
            stats[StatID].RemoveModifier(Modifier);
            base.OnRemove();
        }
    }
}