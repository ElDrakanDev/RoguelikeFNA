using System;
using System.Collections.Generic;
using System.Linq;
using Nez;

namespace RoguelikeFNA.LevelElements
{
    [Serializable]
    public class Spikes : Component, IUpdatable, ITriggerListener
    {
        public int Damage = 1;
        Dictionary<Collider, float> _currentCollisions = new();
        const float DAMAGE_INTERVAL = 0.5f;

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if(
                !_currentCollisions.ContainsKey(other)
                && other.Entity.HasComponent<EntityStats>()
                && other.HasComponent<HealthController>()
            )
                _currentCollisions.Add(other, DAMAGE_INTERVAL);
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            _currentCollisions.Remove(other);
        }

        public void Update()
        {
            _currentCollisions.Keys.Where(
                c => !c.Enabled || c.Entity is null || c.Entity.IsDestroyed
            ).ToList().ForEach(c => _currentCollisions.Remove(c));
            foreach(var collider in _currentCollisions.Keys.ToList())
            {
                if(!_currentCollisions.ContainsKey(collider))
                    continue;

                if(_currentCollisions[collider] >= DAMAGE_INTERVAL)
                {
                    _currentCollisions[collider] = 0f;
                    ApplyDamage(collider);
                }
                else
                {
                    _currentCollisions[collider] += Time.DeltaTime;
                }
            }
        }

        void ApplyDamage(Collider collider)
        {
            var stats = collider.Entity.GetComponent<EntityStats>();
            if(stats != null && !stats.IsIntangible && !stats.IsInvincible)
            {
                collider.Entity.GetComponent<HealthController>()?.Hit(Damage, Entity);
            }
        }
    }
}