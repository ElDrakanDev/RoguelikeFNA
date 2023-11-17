using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace RoguelikeFNA
{
    public class HitboxHandler : Component, IUpdatable
    {
        BoxCollider _collider;
        public List<HitboxGroup> HitboxFrames;
        int _activeIndex = 0;
        public int ActiveIndex { get =>  _activeIndex; set { if(value > 0 && value < HitboxFrames.Count) _activeIndex = value;} }
        HashSet<Entity> _collisions = new HashSet<Entity>();
        HashSet<Entity> _newCollisions = new HashSet<Entity>();
        public int CollidesWithLayers { get => _collider.CollidesWithLayers; set => _collider.CollidesWithLayers = value; }
        public int PhysicsLayer { get => _collider.PhysicsLayer; set => _collider.PhysicsLayer = value; }
        public event Action<Entity> OnCollisionEnter;

        public HitboxHandler() : this(new List<HitboxGroup>()) { }
        public HitboxHandler(List<HitboxGroup> hitboxes)
        {
            HitboxFrames = hitboxes;
        }

        public override void OnAddedToEntity()
        {
            _collider = new BoxCollider(0, 0) { IsTrigger = true};
            Entity.AddComponent(_collider);
        }

        public void Update()
        {
            if(HitboxFrames.Count == 0) return;

            _newCollisions.Clear();

            var rect = HitboxFrames[_activeIndex].Bounds;
            rect.Location = rect.Location + Transform.Position;

            _collider.LocalOffset = rect.Location + rect.Size * 0.5f;
            _collider.SetSize(rect.Width, rect.Height);

            var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, ref rect, CollidesWithLayers);
            Debug.DrawHollowRect(rect, Color.Green);

            foreach(var hitbox in HitboxFrames[_activeIndex].Hitboxes)
            {
                rect = hitbox;
                rect.Location = rect.Location + Transform.Position;

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

