using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace RoguelikeFNA
{
    public interface IProjectileListener
    {
        public void OnLifetimeEnd(Projectile projectile);
        public void OnEntityHit(Projectile projectile, Collider other);
        public void OnGroundHit(Projectile projectile, Collider other);
    }

    public enum GroundHitBehaviour
    {
        Destroy, Bounce, Ignore
    }
    public class Projectile : Component, IUpdatable, ITriggerListener
    {
        HashSet<Collider> _collisions = new HashSet<Collider>();
        public ProjectileMover Mover { get; private set; }
        public float Lifetime;
        public float Damage;
        public Vector2 Velocity;
        public bool FaceVelocity = true;
        public bool ContactDamage = true;
        public GroundHitBehaviour GroundHitBehaviour = GroundHitBehaviour.Destroy;

        public override void OnAddedToEntity()
        {
            Mover = Entity.AddComponent(new ProjectileMover());
            var collider = Entity.AddComponent(new BoxCollider());
            if (
                GroundHitBehaviour != GroundHitBehaviour.Ignore &&
                Physics.BoxcastBroadphaseExcludingSelf(collider, (int)CollisionLayer.Ground).Count > 0
            )
            {
                Entity.Enabled = false;
                Entity.Destroy();
            }
        }

        public void Update()
        {
            if (FaceVelocity)
            {
                Transform.Rotation = Mathf.Atan2(Velocity.Y, Velocity.X);
            }
            Lifetime -= Time.DeltaTime;

            if(Lifetime <= 0)
            {
                Entity.GetComponents<IProjectileListener>().ForEach(x => x.OnLifetimeEnd(this));
                Entity.Destroy();
            }

            Mover.Move(Velocity);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if(_collisions.Contains(other) is false && other.Entity.TryGetComponent(out HealthManager health))
            {
                if(ContactDamage)
                    health.Hit(new DamageInfo(Damage, Entity));

                Entity.GetComponents<IProjectileListener>().ForEach(x => x.OnEntityHit(this, other));
                Entity.Destroy();
            }
            if(GroundHitBehaviour != GroundHitBehaviour.Ignore && Flags.IsFlagSet(other.PhysicsLayer, (int)CollisionLayer.Ground))
            {
                Entity.GetComponents<IProjectileListener>().ForEach(x => x.OnGroundHit(this, other));
                if(GroundHitBehaviour == GroundHitBehaviour.Destroy)
                    Entity.Destroy();
                else if(GroundHitBehaviour == GroundHitBehaviour.Bounce)
                {
                    local.CollidesWith(other, Vector2.Zero, out var result);
                    var translation = result.MinimumTranslationVector;
                    Transform.Position -= translation;

                    if (Math.Sign(translation.X) == Math.Sign(Velocity.X)) Velocity.X *= -1;
                    if (Math.Sign(translation.Y) == Math.Sign(Velocity.Y)) Velocity.Y *= -1;
                }
            }
            _collisions.Add(other);
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
