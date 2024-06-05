using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Nez.Persistence;

namespace RoguelikeFNA
{
    public enum GroundHitBehaviour
    {
        Destroy, Bounce, Ignore
    }
    [Serializable]
    public class Projectile : Component, IUpdatable, ITriggerListener
    {
        HashSet<Collider> _collisions;
        [NsonExclude] public ProjectileMover Mover { get; private set; }
        public float Lifetime;
        public float Damage;
        [NsonExclude] public Vector2 Velocity;
        public bool FaceVelocity = true;
        public bool ContactDamage = true;
        [NsonExclude] public EntityTeam Team;
        [NsonExclude] public int TargetTeams;
        public GroundHitBehaviour GroundHitBehaviour = GroundHitBehaviour.Destroy;
        [NsonExclude] public Entity Owner;

        public override void OnAddedToEntity()
        {
            _collisions = new HashSet<Collider>();
            Mover = Entity.AddComponent(new ProjectileMover());
            Entity.AddComponent(new BoxCollider()
                { CollidesWithLayers = (int)(CollisionLayer.Entity | CollisionLayer.Ground), PhysicsLayer = (int)CollisionLayer.Projectile }
            );
            Mover.Move(Vector2.Zero);
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

            Mover.Move(Velocity * Time.DeltaTime);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if(_collisions.Contains(other) is false
                && other.Entity.TryGetComponent(out EntityStats stats)
                && Flags.IsFlagSet(TargetTeams, (int)stats.Team))
            {
                if(ContactDamage)
                    stats.HealthManager?.Hit(new DamageInfo(Damage, Entity));

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

        public void SetValuesFromEntityStats(EntityStats stats)
        {
            Owner = stats.Entity;
            Team = stats.Team;
            TargetTeams = stats.TargetTeams;
        }
    }
}
