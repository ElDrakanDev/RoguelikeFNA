using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace RoguelikeFNA
{
    public class HitboxHandler : Component, IUpdatable
    {
        BoxCollider _collider;
        [InspectorSerializable] public Dictionary<string, List<HitboxGroup>> AnimationsHitboxes;
        string _activeAnimation;
        public string ActiveAnimation
        { 
            get => _activeAnimation;
            set { if (AnimationsHitboxes.ContainsKey(value)) { _activeAnimation = value; } } 
        }
        int _activeIndex = 0;
        public int ActiveIndex { get =>  _activeIndex; set { if(value > 0 && value < AnimationsHitboxes.Count) _activeIndex = value;} }
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
        }

        public void Update()
        {
            if(AnimationsHitboxes.Count == 0
                || ActiveAnimation is null
                || AnimationsHitboxes.ContainsKey(ActiveAnimation) is false
                || ActiveIndex >= AnimationsHitboxes[ActiveAnimation].Count
            )
                return;

            _newCollisions.Clear();

            var hitboxGroup = AnimationsHitboxes[ActiveAnimation][ActiveIndex];
            var rect = hitboxGroup.Bounds;

            _collider.LocalOffset = rect.Location + rect.Size * 0.5f;
            _collider.SetSize(rect.Width, rect.Height);

            rect.Location = rect.Location * _collider.Entity.LocalScale + Transform.Position;
            rect.Size = rect.Size * _collider.Entity.LocalScale;
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

