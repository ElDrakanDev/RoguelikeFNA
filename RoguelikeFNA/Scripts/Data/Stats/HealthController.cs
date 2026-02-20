using System;
using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class DamageInfo
    {
        public int Damage;
        public Vector2 Knockback;
        public object Source;
        public bool Canceled = false;

        public DamageInfo(int damage, Entity source, Vector2 knockback = default)
        {
            Damage = damage;
            Knockback = knockback;
            Source = source;
        }

        public DamageInfo(float damage, Entity source, Vector2 knockback = default) : this(Mathf.RoundToInt(damage), source, knockback) { }
    }

    public class HealInfo
    {
        public int Amount;
        public object Source;

        public HealInfo(int amount, Entity source)
        {
            Amount = amount;
            Source = source;
        }
    }
    public class DeathInfo
    {
        public bool Canceled = false;
        public object Source;

        public DeathInfo(object source)
        {
            Source = source;
        }
    }
    [Serializable]
    public class HealthController : Component
    {
        int _health;
        public Stat MaxHealth { get; private set; }
        public bool IsAlive => Health > 0;
        public int Health
        {
            get => _health;
            private set
            {
                if (value < 0) _health = 0;
                else if (value > MaxHealth) _health = Mathf.RoundToInt(MaxHealth);
                else _health = value;
            }
        }
        public HealthController() { }

        /// <summary>
        /// Pre damage taken event.
        /// </summary>
        public event Action<DamageInfo> preDamageTaken;
        /// <summary>
        /// Event invoked after taking damage
        /// </summary>
        public event Action<DamageInfo> onDamageTaken;
        /// <summary>
        /// Pre healing event
        /// </summary>
        public event Action<HealInfo> preHeal;
        /// <summary>
        /// Pre death event
        /// </summary>
        public event Action<DeathInfo> preDeath;
        /// <summary>
        /// Event invoked when entity dies
        /// </summary>
        public event Action<DeathInfo> onDeath;

        public override void OnAddedToEntity()
        {
            MaxHealth ??= Entity.GetComponent<EntityStats>()?[StatID.Health] ?? new Stat(1, Entity);
            Health = Mathf.RoundToInt(MaxHealth);
        }

        public void Hit(DamageInfo info)
        {
            preDamageTaken?.Invoke(info);
            if (info.Canceled is false) {
                Health -= info.Damage;
                onDamageTaken?.Invoke(info);
                if (Health == 0) Die(new DeathInfo(info.Source));
            }
        }
        public void Heal(HealInfo info)
        {
            preHeal?.Invoke(info);
            Health += info.Amount;
        }
        public void Die(DeathInfo info)
        {
            preDeath?.Invoke(info);
            if(info.Canceled is false) onDeath?.Invoke(info);
        }
    }
}
