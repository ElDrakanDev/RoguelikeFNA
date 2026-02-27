using System;
using Nez;

namespace RoguelikeFNA
{
    [Serializable]
    public class IframeHandler : Component, IPreDamageListener, IUpdatable
    {
        public bool IsInvulnerable { get; private set; }
        float _timeLeft;
        public float IframeDuration = 1f;

        public float TimeLeft => _timeLeft;

        public override void OnAddedToEntity()
        {
            Entity.GetComponent<HealthController>()?.PreDamageListeners.Add(this);
        }

        public override void OnRemovedFromEntity()
        {
            Entity.GetComponent<HealthController>()?.PreDamageListeners.Remove(this);
        }

        public void Update()
        {
            if (!IsInvulnerable)
                return;

            _timeLeft -= Time.DeltaTime;
            if (_timeLeft <= 0)
                IsInvulnerable = false;
        }

        public void PreDamageTaken(DamageInfo damageInfo)
        {
            if(IsInvulnerable)
            {
                damageInfo.Canceled = true;
                return;
            }
            IsInvulnerable = true;
            _timeLeft = IframeDuration;
        }
    }
}