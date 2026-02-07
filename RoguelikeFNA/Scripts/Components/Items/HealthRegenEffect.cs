using Nez;
using RoguelikeFNA.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeFNA.Items
{
    public class HealthRegenEffect : ItemEffect
    {
        [Inspectable] public float RegenRate;
        [Inspectable] public int RegenAmount;

        float _counter;

        public override string DescriptionId => "regen_effect_desc";

        public override string GetDescription()
        {
            return string.Format(base.GetDescription(), RegenAmount, RegenRate);
        }

        public override void Tick()
        {
            _counter += Time.DeltaTime;
            if(_counter >= RegenRate)
            {
                Owner.GetComponent<HealthController>()?.Heal(new HealInfo(RegenAmount, Owner));
                _counter = 0;
            }
        }
    }
}
