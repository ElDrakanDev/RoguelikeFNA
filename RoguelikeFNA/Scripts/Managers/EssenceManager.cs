using System;
using Nez;

namespace RoguelikeFNA
{
    public class EssenceManager : GlobalManager
    {
        public int Essence { get; private set; }

        public void AddEssence(int essence)
        {
            Essence += essence;
        }

        public bool TrySpendEssence(int amount)
        {
            if (!CanAfford(amount))
                return false;
            SpendEssence(amount);
            return true;
        }

        public void SpendEssence(int amount)
        {
            if (!CanAfford(amount))
                throw new ArgumentException("Essence can't be negative");
            Essence -= amount;
        }

        public bool CanAfford(int amount) => Essence - amount > 0;

        public void Reset()
        {
            Essence = 0;
        }
    }
}