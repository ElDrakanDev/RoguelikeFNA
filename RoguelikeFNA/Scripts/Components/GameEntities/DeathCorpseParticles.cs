using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Tweens;

namespace RoguelikeFNA.Entities
{
    [Serializable]
    public class DeathCorpseParticles : Component, IDamageListener, IDeathListener
    {
        [Serializable]
        class CorpseParticlePhysics : Component, IUpdatable
        {
            public float Gravity = 450f;
            public float BounceSpeedRetention = 0.65f;
            public float BounceSpinRetention = 0.6f;
            public float StopSpeedThreshold = 6f;
            public float StopSpinThreshold = 1.5f;
            public float SleepColorRetention = 0.6f;
            public float SleepFadeDuration = 0.2f;

            Mover _mover;
            PhysicsBody _physicsBody;
            Collider _collider;
            float _angularVelocity;
            Vector2 _initialVelocity;
            SpriteRenderer _renderer;
            bool _isSleeping;

            public CorpseParticlePhysics(Vector2 velocity, float angularVelocity)
            {
                _angularVelocity = angularVelocity;
                _initialVelocity = velocity;
            }

            public override void OnAddedToEntity()
            {
                _mover = Entity.GetOrCreateComponent<Mover>();
                _physicsBody = Entity.GetOrCreateComponent<PhysicsBody>();
                _physicsBody.MoveOnUpdate = false;
                _physicsBody.Velocity = _initialVelocity;
                _collider = Entity.GetComponent<Collider>();
                _renderer = Entity.GetComponent<SpriteRenderer>();
            }

            public void Update()
            {
                if (_isSleeping)
                    return;

                _physicsBody.Velocity.Y += Gravity * Time.DeltaTime;

                var motion = _physicsBody.CalculateMotion();
                if (_mover.Move(motion, out var collision))
                    Bounce(collision);

                Entity.Transform.Rotation += _angularVelocity * Time.DeltaTime;
            }

            void Bounce(CollisionResult collision)
            {
                var translation = collision.MinimumTranslationVector;
                var collidedBelow = translation.Y > 0f;

                if (translation.X != 0f && Math.Sign(translation.X) == Math.Sign(_physicsBody.Velocity.X))
                    _physicsBody.Velocity.X = -_physicsBody.Velocity.X;

                if (translation.Y != 0f && Math.Sign(translation.Y) == Math.Sign(_physicsBody.Velocity.Y))
                    _physicsBody.Velocity.Y = -_physicsBody.Velocity.Y;

                _physicsBody.Velocity *= BounceSpeedRetention;
                _angularVelocity *= BounceSpinRetention;

                if (collidedBelow
                    && _physicsBody.Velocity.LengthSquared() <= StopSpeedThreshold * StopSpeedThreshold
                    && Math.Abs(_angularVelocity) <= StopSpinThreshold)
                {
                    PutParticleToSleep();
                }
            }

            void PutParticleToSleep()
            {
                _isSleeping = true;
                _physicsBody.Velocity = Vector2.Zero;
                _angularVelocity = 0f;

                if (_renderer != null)
                {
                    var targetColor = _renderer.Color;
                    targetColor *= SleepColorRetention;
                    _renderer.TweenColorTo(targetColor, SleepFadeDuration)
                        .SetEaseType(EaseType.QuadOut)
                        .Start();
                }

                Entity.RemoveComponent<PhysicsBody>();
                Entity.RemoveComponent<Mover>();
                if (_collider != null)
                    Entity.RemoveComponent(_collider);

                Entity.RemoveComponent(this);
            }
        }

        public string[] Sprites = Array.Empty<string>();

        public float RandomVelocityMin = 30f;
        public float RandomVelocityMax = 140f;
        public float RandomVelocityMultiplier = 1f;
        public float RotationForcePerVelocity = 0.03f;
        public float BounceSpeedRetention = 0.65f;
        public float BounceSpinRetention = 0.6f;
        public float Gravity = 450f;
        public float StopSpeedThreshold = 12f;
        public float StopSpinThreshold = 1.5f;
        public float SleepColorRetention = 0.7f;
        public float SleepFadeDuration = 0.2f;
        public int ColliderSize = 4;

        Vector2 _lastKnockback;
        HealthController _healthController;

        public override void OnAddedToEntity()
        {
            _healthController = Entity.GetComponent<HealthController>();
            if (_healthController == null)
                return;

            _healthController.DamageListeners.Add(this);
            _healthController.DeathListeners.Add(this);
        }

        public override void OnRemovedFromEntity()
        {
            if (_healthController == null)
                return;

            _healthController.DamageListeners.Remove(this);
            _healthController.DeathListeners.Remove(this);
        }

        public void OnDamageTaken(DamageInfo damageInfo)
        {
            _lastKnockback = damageInfo.Knockback;
        }

        public void OnDeath(DeathInfo deathInfo)
        {
            if (Sprites == null || Sprites.Length == 0)
                return;

            var parent = Entity.Parent;
            for (var i = 0; i < Sprites.Length; i++)
            {
                var sprite = Sprites[i];
                if (sprite == null)
                    continue;

                SpawnPiece(sprite, parent, i);
            }
        }

        void SpawnPiece(string sprite, Transform parent, int index)
        {
            var randomDirection = Nez.Random.NextUnitVector();
            var randomSpeed = Nez.Random.Range(RandomVelocityMin, RandomVelocityMax) * RandomVelocityMultiplier;
            var spawnVelocity = randomDirection * randomSpeed + _lastKnockback;

            var spinSign = Math.Sign(randomDirection.X);
            var angularVelocity = spawnVelocity.Length() * RotationForcePerVelocity * spinSign;

            var piece = new Entity($"{Entity.Name}_corpse_piece_{index}")
                .SetPosition(Entity.Transform.Position)
                .SetParent(Entity.Parent);

            if (parent != null)
                piece.SetParent(parent);

            piece.AddComponent(new SpriteRenderer(Entity.Scene.Content.Load<Texture2D>(sprite)));

            var collider = new BoxCollider(ColliderSize, ColliderSize)
            {
                PhysicsLayer = (int)CollisionLayer.None,
                CollidesWithLayers = (int)CollisionLayer.Ground,
                IsTrigger = false
            };
            piece.AddComponent(collider);

            piece.AddComponent(new CorpseParticlePhysics(spawnVelocity, angularVelocity)
            {
                Gravity = Gravity,
                BounceSpeedRetention = BounceSpeedRetention,
                BounceSpinRetention = BounceSpinRetention,
                StopSpeedThreshold = StopSpeedThreshold,
                StopSpinThreshold = StopSpinThreshold,
                SleepColorRetention = SleepColorRetention,
                SleepFadeDuration = SleepFadeDuration
            });

            Entity.Scene.AddEntity(piece);
        }
    }
}