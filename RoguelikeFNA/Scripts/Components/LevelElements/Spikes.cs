using System;
using System.Collections.Generic;
using Nez;

namespace RoguelikeFNA.LevelElements
{
    [Serializable]
    public class Spikes : Component, IUpdatable, ITriggerListener
    {
        public int Damage = 1;
        HashSet<Collider> _currentCollisions = new();

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if(
                !_currentCollisions.Contains(other)
                && other.Entity.HasComponent<EntityStats>()
                && other.HasComponent<HealthController>()
            )
                _currentCollisions.Add(other);
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            _currentCollisions.Remove(other);
        }

        public void Update()
        {
            _currentCollisions.RemoveWhere(c => !c.Enabled || c.Entity is null || c.Entity.IsDestroyed);
            foreach(var collider in _currentCollisions)
            {
                var stats = collider.Entity.GetComponent<EntityStats>();
                if(stats != null && !stats.IsIntangible && !stats.IsInvincible)
                {
                    collider.Entity.GetComponent<HealthController>()?.Hit(Damage, Entity);
                }
            }
        }
    }
}