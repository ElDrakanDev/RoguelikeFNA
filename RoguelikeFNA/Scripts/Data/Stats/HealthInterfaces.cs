namespace RoguelikeFNA {
    public interface IPreDamageListener
    {
        void PreDamageTaken(DamageInfo damageInfo);
    }

    public interface IDamageListener
    {
        void OnDamageTaken(DamageInfo damageInfo);
    }

    public interface IPreHealListener
    {
        void PreHealed(HealInfo healInfo);
    }

    public interface IHealListener
    {
        void OnHealed(HealInfo healInfo);
    }

    public interface IPreDeathListener
    {
        void PreDeath(DeathInfo deathInfo);
    }

    public interface IDeathListener
    {
        void OnDeath(DeathInfo deathInfo);
    }
}