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
            if (Essence - amount < 0)
                return false;
            SpendEssence(amount);
            return true;
        }

        public void SpendEssence(int amount)
        {
            if (Essence - amount < 0)
                throw new ArgumentException("Essence can't be negative");
        }

        public void Reset()
        {
            Essence = 0;
        }
    }
}