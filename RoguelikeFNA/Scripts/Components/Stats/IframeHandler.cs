using System;
using Nez;

namespace RoguelikeFNA
{
    [Serializable]
    public class IframeHandler : Component, IPreDamageListener, IUpdatable
    {
        public const float DEFAULT_IFRAME_DURATION = 0.8f;
        public const float MIN_IFRAME_DURATION = 0.1f;
        public const float MAX_IFRAME_DURATION = 10f;
        public bool IsInvulnerable => _condition.Value;
        float _timeLeft;

        public float IframeDuration => _stats[StatID.Iframes].Value;
        public float TimeLeft => _timeLeft;
        EntityStats _stats;
        StatBool.Condition _condition = new();
        SpriteBlinking _spriteBlinking;

        public override void OnAddedToEntity()
        {
            Entity.GetComponent<HealthController>()?.PreDamageListeners.Add(this);
            _spriteBlinking = Entity.GetOrCreateComponent<SpriteBlinking>();
            _stats = Entity.GetComponent<EntityStats>();
            if(!_stats.TryGetStat(StatID.Iframes, out var _))
            {
                _stats.Stats[StatID.Iframes] = new Stat(DEFAULT_IFRAME_DURATION, Entity, MIN_IFRAME_DURATION, MAX_IFRAME_DURATION);
            }
            _stats.IsIntangible.Conditions.Add(_condition);
        }

        public override void OnRemovedFromEntity()
        {
            Entity.GetComponent<HealthController>()?.PreDamageListeners.Remove(this);
            _stats.IsIntangible.Conditions.Remove(_condition);
        }

        public void Update()
        {
            if (!IsInvulnerable)
                return;

            _timeLeft -= Time.DeltaTime;
            if (_timeLeft <= 0)
            {
                _condition.Value = false;
                _spriteBlinking.IsActive = false;
            }
        }

        public void PreDamageTaken(DamageInfo damageInfo)
        {
            if(IsInvulnerable)
            {
                damageInfo.Canceled = true;
                return;
            }
            _condition.Value = true;
            _spriteBlinking.IsActive = true;
            _timeLeft = IframeDuration;
        }
    }
}