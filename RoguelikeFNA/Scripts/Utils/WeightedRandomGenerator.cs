using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeFNA.Utils
{
    public class WeightedItem<T>
    {
        public readonly T Value;
        public readonly int Weight;

        public WeightedItem(T item, int weight)
        {
            Value = item;
            Weight = weight;
        }
    }

    public class WeightedRandomGenerator<T>
    {
        private readonly RNG _rng;
        public List<WeightedItem<T>> WeightedItems = new List<WeightedItem<T>>();

        float _totalWeight;

        public WeightedRandomGenerator() { }
        public WeightedRandomGenerator(RNG rng) => this._rng = rng;

        public void AddItem(T item, int weight)
        {
            WeightedItems.Add(new WeightedItem<T>(item, weight));
            _totalWeight += weight;
        }

        public void RemoveItem(WeightedItem<T> item)
        {
            if (WeightedItems.Remove(item))
                _totalWeight -= item.Weight;
        }

        public float GetPercentChance(WeightedItem<T> item)
        {
            return (item.Weight / _totalWeight) * 100;
        }

        public WeightedItem<T> GetRandomItem()
        {
            float randomNum;
            if(_rng != null)
                randomNum = _rng.FRange(0, _totalWeight);
            else
                randomNum = Nez.Random.NextFloat(_totalWeight);

            foreach (var weightedItem in WeightedItems)
            {
                if (randomNum < weightedItem.Weight)
                {
                    return weightedItem;
                }
                randomNum -= weightedItem.Weight;
            }

            return null;
        }

        public WeightedItem<T> GetRandomItem(Func<WeightedItem<T>, bool> predicate)
        {
            float randomNum;
            var items = WeightedItems.Where(predicate).ToArray();
            var total = items.Sum(x => x.Weight);

            if (_rng != null)
                randomNum = _rng.FRange(0, total);
            else
                randomNum = Nez.Random.NextFloat(total);

            foreach (var weightedItem in items)
            {
                if (randomNum < weightedItem.Weight)
                {
                    return weightedItem;
                }
                randomNum -= weightedItem.Weight;
            }

            return null;
        }
    }
}
