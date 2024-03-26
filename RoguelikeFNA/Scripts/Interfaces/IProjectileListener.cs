using Nez;

namespace RoguelikeFNA
{
    public interface IProjectileListener
    {
        public void OnLifetimeEnd(Projectile projectile);
        public void OnEntityHit(Projectile projectile, Collider other);
        public void OnGroundHit(Projectile projectile, Collider other);
    }
}
