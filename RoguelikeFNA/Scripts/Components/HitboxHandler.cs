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
        [InspectorSerializable] public Dictionary<string, List<HitboxGroup>> AnimationsHitboxes;
        public string AnimationName => Animator.CurrentAnimationName;
        public int SpriteIndex => Animator.CurrentFrame;
        public int CollidesWithLayers { get => _collider.CollidesWithLayers; set => _collider.CollidesWithLayers = value; }
        public int PhysicsLayer { get => _collider.PhysicsLayer; set => _collider.PhysicsLayer = value; }
        public event Action<Entity> OnCollisionEnter;
        
        BoxCollider _collider;
        HashSet<Entity> _collisions = new HashSet<Entity>();
        HashSet<Entity> _newCollisions = new HashSet<Entity>();
        bool _clear;

        public HitboxHandler() : this(new Dictionary<string, List<HitboxGroup>>()) { }
        public HitboxHandler(Dictionary<string, List<HitboxGroup>> hitboxes)
        {
            AnimationsHitboxes = hitboxes;
            _collider = new BoxCollider(0, 0) { IsTrigger = true};
        }

        public override void OnAddedToEntity()
        {
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

            if (_clear)
            {
                _clear = false;
                _collisions.Clear();
                _newCollisions.Clear();
            }

            _newCollisions.Clear();

            var hitboxGroup = AnimationsHitboxes[AnimationName][SpriteIndex];
            var rect = hitboxGroup.Bounds;

            _collider.LocalOffset = rect.Location + rect.Size * 0.5f;
            _collider.SetSize(rect.Width, rect.Height);

            rect = rect.Transformed(Transform);
            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, ref rect, CollidesWithLayers);
            Debug.DrawHollowRect(rect, Color.Yellow);

            foreach (var hitbox in hitboxGroup.Hitboxes)
            {
                rect = hitbox;

                _collider.LocalOffset = hitbox.Location + rect.Size * 0.5f;
                _collider.SetSize(rect.Width, rect.Height);

                foreach(var neighbor in neighbors)
                {
                    if(neighbor.Entity != Entity && _collisions.Contains(neighbor.Entity) is false && _collider.Overlaps(neighbor))
                    {
                        _collisions.Add(neighbor.Entity);
                        _newCollisions.Add(neighbor.Entity);
                    }
                }
                rect = rect.Transformed(Transform);
                Debug.DrawHollowRect(rect, Color.Yellow);
            }

            // Events
            foreach (var collision in _newCollisions)
            {
                OnCollisionEnter?.Invoke(collision);
            }
        }

        public void ClearCollisions() => _clear = true;
    }
}

