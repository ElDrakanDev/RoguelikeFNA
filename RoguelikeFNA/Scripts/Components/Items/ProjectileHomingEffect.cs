using Nez;

namespace RoguelikeFNA.Items
{
    [System.Serializable]
    public class ProjectileHomingEffect : ItemEffect, IProjectileShootListener
    {
        [Inspectable] public float HomingRange;
        [Inspectable] public float HomingSpeed;
        public override string DescriptionId => "homing_effect_desc";

        public void Fire(Projectile projectile)
        {
            projectile.Entity.AddComponent(new ProjecitleHoming() { HomingRange = HomingRange, AttractSpeed = HomingSpeed });
        }
    }
}
