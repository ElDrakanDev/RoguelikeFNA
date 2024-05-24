using Nez;
using System.Collections.Generic;


namespace RoguelikeFNA
{
    public class RNGManager : GlobalManager
    {
        public enum GeneratorTypes
        {
            Level, Item
        }

        Dictionary<GeneratorTypes, RNG> _generators = new Dictionary<GeneratorTypes, RNG>();
        public int Seed { get; private set; }

        public override void OnEnabled()
        {
            SetSeed(Random.Range(int.MinValue, int.MaxValue));
        }

        /// <summary>
        /// Sets the seed for all RNGs
        /// </summary>
        /// <param name="seed"></param>
        public void SetSeed(int seed)
        {
            Seed = seed;
            foreach(var key in _generators.Keys)
            {
                _generators[key] = new RNG(seed);
            }
        }

        /// <summary>
        /// Gets the RNG associated with the name. Note that if you cache the result and the seed is changed you will have the old RNG
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public RNG GetRNG(GeneratorTypes type)
        {
            if(_generators.TryGetValue(type, out RNG generator))
                return generator;
            generator = new RNG(Seed);
            _generators[type] = generator;
            return generator;
        }
    }
}
