using Nez;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace RoguelikeFNA
{
    public class HitboxHandler : Component, IUpdatable
    {
        BoxCollider _collider;
        public List<HitboxGroup> HitboxFrames = new List<HitboxGroup>();
        int _activeIndex = 0;
        public int ActiveIndex { get =>  _activeIndex; set { if(value > 0 && value < HitboxFrames.Count) _activeIndex = value;} }
        HashSet<Collider> _collisions = new HashSet<Collider>();
        HashSet<Collider> _prevCollisions = new HashSet<Collider>();
        public int CollidesWithLayers { get => _collider.CollidesWithLayers; set => _collider.CollidesWithLayers = value; }
        public int PhysicsLayer { get => _collider.PhysicsLayer; set => _collider.PhysicsLayer = value; }
        public event Action<Collider> OnCollisionEnter;
        public event Action<Collider> OnCollisionStay;
        public event Action<Collider> OnCollisionExit;


        public override void OnAddedToEntity()
        {
            _collider = new BoxCollider(0, 0) { IsTrigger = true};
            Entity.AddComponent(_collider);
        }

        public void Update()
        {
            if(HitboxFrames.Count == 0) return;

            _prevCollisions.Clear();
            _prevCollisions.UnionWith(_collisions);
            _collisions.Clear();

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

                bool anycollisions = false;

                foreach(var neighbor in neighbors)
                {
                    if(_collisions.Contains(neighbor) is false && _collider.Overlaps(neighbor))
                    {
                        anycollisions = true;
                        _collisions.Add(neighbor);
                    }
                }

                Debug.DrawHollowRect(rect, anycollisions ? Color.Yellow : Color.White);    
            }

            // Events
            foreach(var collision in _collisions)
            {
                if(_prevCollisions.Contains(collision))
                    OnCollisionStay?.Invoke(collision);
                else
                    OnCollisionEnter?.Invoke(collision);
            }
            foreach(var collision in _prevCollisions)
                if(_collisions.Contains(collision) is false)
                    OnCollisionExit?.Invoke(collision);
        }
    }
}

