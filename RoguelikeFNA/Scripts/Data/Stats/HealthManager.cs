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

        public DamageInfo(int damage, object source, Vector2 knockback = default)
        {
            Damage = damage;
            Knockback = knockback;
            Source = source;
        }

        public DamageInfo(float damage, object source, Vector2 knockback = default) : this(Mathf.RoundToInt(damage), source, knockback) { }
    }

    public class HealInfo
    {
        public int Amount;
        public object Source;

        public HealInfo(int amount, object source)
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
    public class HealthManager : Component
    {
        int _health;
        public readonly Stat MaxHealth;
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
        public HealthManager(int health, int maxHealth, int minMaxHealth = 1)
        {
            MaxHealth = new Stat(maxHealth, Entity, minMaxHealth);
            Health = health;
        }
        public HealthManager(int health, Stat maxHealth)
        {
            MaxHealth = maxHealth;
            Health = health;
        }
        public HealthManager(int maxHealth, int minMaxHealth = 1)
        {
            MaxHealth = new Stat(maxHealth, Entity, minMaxHealth);
            Health = Mathf.RoundToInt(MaxHealth);
        }
        public HealthManager(Stat maxHealth)
        {
            MaxHealth = maxHealth;
            Health = Mathf.RoundToInt(MaxHealth);
        }

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
        public event Action<object> onDeath;

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
            if(info.Canceled is false) onDeath?.Invoke(info.Source);
        }
    }
}
