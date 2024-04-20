using Nez;
using System.Collections.Generic;


namespace RoguelikeFNA
{
    public interface IRNGDependent
    {
        public RNG RNG { get; set; }
    }
    public class RNGManager : GlobalManager
    {
        public List<IRNGDependent> _rngDependents;

        public override void OnEnabled()
        {
            _rngDependents = Core.GetGlobalManagers<IRNGDependent>();
            SetSeed(Random.Range(int.MinValue, int.MaxValue));
        }

        public void SetSeed(int seed)
        {
            foreach(var dependent in _rngDependents)
            {
                dependent.RNG = new RNG(seed);
            }
        }
    }
}
