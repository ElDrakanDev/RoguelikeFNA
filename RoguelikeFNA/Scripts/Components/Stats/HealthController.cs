using System;
using System.Collections.Generic;
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
    public enum DeathBehaviour
    {
        None, Destroy
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
        public DeathBehaviour DeathBehaviour = DeathBehaviour.Destroy;
        public HealthController() { }

        public List<IPreDamageListener> PreDamageListeners {get; protected set;}
        public List<IDamageListener> DamageListeners {get; protected set;}
        public List<IPreHealListener> PreHealListeners {get; protected set;}
        public List<IHealListener> HealListeners {get; protected set;}
        public List<IPreDeathListener> PreDeathListeners {get; protected set;}
        public List<IDeathListener> DeathListeners {get; protected set;}

        public override void Initialize()
        {
            PreDamageListeners = ListPool<IPreDamageListener>.Obtain();
            DamageListeners = ListPool<IDamageListener>.Obtain();
            PreHealListeners = ListPool<IPreHealListener>.Obtain();
            HealListeners = ListPool<IHealListener>.Obtain();
            PreDeathListeners = ListPool<IPreDeathListener>.Obtain();
            DeathListeners = ListPool<IDeathListener>.Obtain();
        }

        public override void OnAddedToEntity()
        {
            MaxHealth ??= Entity.GetComponent<EntityStats>()?[StatID.Health] ?? new Stat(1, Entity);
            Health = Mathf.RoundToInt(MaxHealth);
        }

        public override void OnRemovedFromEntity()
        {
            ListPool<IPreDamageListener>.Free(PreDamageListeners);
            ListPool<IDamageListener>.Free(DamageListeners);
            ListPool<IHealListener>.Free(HealListeners);
            ListPool<IPreDeathListener>.Free(PreDeathListeners);
            ListPool<IDeathListener>.Free(DeathListeners);
        }

        public bool Hit(DamageInfo info)
        {
            foreach(var listener in PreDamageListeners)
            {
                if(info.Canceled) return false;
                listener.PreDamageTaken(info);
            }

            if (info.Canceled)
                return false;

            Health -= info.Damage;
            foreach(var listener in DamageListeners)
            {
                listener.OnDamageTaken(info);
            }
            if (Health == 0) Die(new DeathInfo(info.Source));
            return true;
        }

        public void Heal(HealInfo info)
        {
            foreach(var listener in PreHealListeners)
            {
                listener.PreHealed(info);
            }
        
            Health += info.Amount;
            foreach(var listener in HealListeners)
            {
                listener.OnHealed(info);
            }
        }

        public void Die(DeathInfo info)
        {
            foreach(var listener in PreDeathListeners)
            {
                if(info.Canceled) return;
                listener.PreDeath(info);
            }
            if(info.Canceled)
                return;

            foreach(var listener in DeathListeners)
            {
                listener.OnDeath(info);
            }
            if(DeathBehaviour == DeathBehaviour.Destroy)
            {
                Entity.Destroy();
            }
        }
    }
}
