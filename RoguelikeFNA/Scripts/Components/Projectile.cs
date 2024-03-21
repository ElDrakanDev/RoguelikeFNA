using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using ImGuiNET;

namespace RoguelikeFNA
{
    public enum GroundHitBehaviour
    {
        Destroy, Bounce, Ignore
    }
    public class Projectile : Component, IUpdatable, ITriggerListener
    {
        HashSet<Collider> _collisions = new HashSet<Collider>();
        public ProjectileMover Mover { get; private set; }
        public float damage;
        public Vector2 Velocity;
        public event Action<Collider, Projectile> OnEntityHit;
        public event Action<Collider, Projectile> OnGroundHit;
        public bool FaceVelocity = true;
        public GroundHitBehaviour GroundHitBehaviour = GroundHitBehaviour.Destroy;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
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
            Mover.Move(Velocity);
        }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if(_collisions.Contains(other) is false && other.Entity.TryGetComponent(out HealthManager health))
            {
                health.Hit(new DamageInfo(damage, Entity));
                OnEntityHit?.Invoke(other, this);
                Entity.Destroy();
            }
            if(GroundHitBehaviour != GroundHitBehaviour.Ignore && Flags.IsFlagSet(other.PhysicsLayer, (int)CollisionLayer.Ground))
            {
                OnGroundHit?.Invoke(other, this);
                if(GroundHitBehaviour == GroundHitBehaviour.Destroy)
                    Entity.Destroy();
                else if(GroundHitBehaviour == GroundHitBehaviour.Bounce)
                {
                    local.CollidesWith(other, Vector2.Zero, out var result);
                    if (result.MinimumTranslationVector.X != 0) Velocity.X *= -1;
                    if(result.MinimumTranslationVector.Y != 0) Velocity.Y *= -1;
                }
            }
            _collisions.Add(other);
        }

        public void OnTriggerExit(Collider other, Collider local) { }
    }
}
