using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Nez.Sprites;

namespace RoguelikeFNA
{
    public class HitboxHandler : Component, IUpdatable
    {
        /// <summary>
        /// The animator this handler's hitboxes will be bound to
        /// </summary>
        public SpriteAnimator Animator;
        BoxCollider _collider;
        [InspectorSerializable] public Dictionary<string, List<HitboxGroup>> AnimationsHitboxes;
        public string AnimationName => Animator.CurrentAnimationName;
        public int SpriteIndex => Animator.CurrentFrame;
        HashSet<Entity> _collisions = new HashSet<Entity>();
        HashSet<Entity> _newCollisions = new HashSet<Entity>();
        public int CollidesWithLayers { get => _collider.CollidesWithLayers; set => _collider.CollidesWithLayers = value; }
        public int PhysicsLayer { get => _collider.PhysicsLayer; set => _collider.PhysicsLayer = value; }
        public event Action<Entity> OnCollisionEnter;

        public HitboxHandler() : this(new Dictionary<string, List<HitboxGroup>>()) { }
        public HitboxHandler(Dictionary<string, List<HitboxGroup>> hitboxes)
        {
            AnimationsHitboxes = hitboxes;
        }

        public override void OnAddedToEntity()
        {
            _collider = new BoxCollider(0, 0) { IsTrigger = true};
            Entity.AddComponent(_collider);
            Animator = Animator ?? Entity.GetComponent<SpriteAnimator>();
        }

        public void Update()
        {
            if( Animator is null
                || AnimationsHitboxes.Count == 0
                || AnimationsHitboxes.ContainsKey(AnimationName) is false
                || SpriteIndex >= AnimationsHitboxes[AnimationName].Count
            )
                return;

            _newCollisions.Clear();

            var hitboxGroup = AnimationsHitboxes[AnimationName][SpriteIndex];
            var rect = hitboxGroup.Bounds;

            _collider.LocalOffset = rect.Location + rect.Size * 0.5f;
            _collider.SetSize(rect.Width, rect.Height);

            rect.Location = rect.Location * _collider.Entity.Scale + Transform.Position;
            rect.Size = rect.Size * _collider.Entity.Scale;
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, ref rect, CollidesWithLayers);
            Debug.DrawHollowRect(rect, Color.Green);

            foreach(var hitbox in hitboxGroup.Hitboxes)
            {
                rect = hitbox;

                _collider.LocalOffset = hitbox.Location + rect.Size * 0.5f;
                _collider.SetSize(rect.Width, rect.Height);

                foreach(var neighbor in neighbors)
                {
                    if(_collisions.Contains(neighbor.Entity) is false && _collider.Overlaps(neighbor))
                    {
                        _collisions.Add(neighbor.Entity);
                        _newCollisions.Add(neighbor.Entity);
                    }
                }
                rect.Location = Transform.Position + rect.Location * Entity.Scale;
                rect.Size = rect.Size * Entity.Scale;
                Debug.DrawHollowRect(rect, Color.Yellow);    
            }

            // Events
            foreach (var collision in _newCollisions)
            {
                OnCollisionEnter?.Invoke(collision);
            }
        }

        public void ClearCollisions() => _collisions.Clear();
    }
}

