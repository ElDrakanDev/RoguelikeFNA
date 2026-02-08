using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;

namespace RoguelikeFNA
{
    public class GameEntityManager : GlobalManager
    {
        HashSet<GameEntity> _entitySet = new();
        List<GameEntity> _entities = new();

        public static List<GameEntity> Entities => _instance._entities;

        static GameEntityManager _instance;

        public event Action<GameEntity> OnEntityAdded;
        public event Action<GameEntity> OnEntityRemoved;

        public override void OnEnabled()
        {
            if(_instance != null)
            {
                Debug.Warn("Multiple instances of GameEntityManager detected. This is not intended and may cause issues.");
            }
            _instance = this;
        }

        public static void RegisterEntity(GameEntity entity)
        {
            if (!_instance._entitySet.Contains(entity))
            {
                _instance._entitySet.Add(entity);
                _instance._entities.Add(entity);
                _instance.OnEntityAdded?.Invoke(entity);
            }
        }

        public static void UnregisterEntity(GameEntity entity)
        {
            if (_instance._entitySet.Contains(entity))
            {
                _instance._entitySet.Remove(entity);
                _instance._entities.Remove(entity);
                _instance.OnEntityRemoved?.Invoke(entity);
            }
        }

        public static void Clear()
        {
            _instance._entitySet.Clear();
            _instance._entities.Clear();
        }
    }

    public static class GameEntityExtensions
    {
        public static IEnumerable<GameEntity> OfTeam(this IEnumerable<GameEntity> entities, int teamMask)
        {
            foreach (var entity in entities)
            {
                if (Flags.IsFlagSet(teamMask, (int)entity.Stats.Team))
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> InRange(this IEnumerable<GameEntity> entities, Vector2 position, float range)
        {
            foreach (var entity in entities)
            {
                if (entity.InRange(position, range))
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> LineOfSight(this IEnumerable<GameEntity> entities, Vector2 position)
        {
            foreach (var entity in entities)
            {
                if (entity.LineOfSight(position))
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> Alive(this IEnumerable<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.IsAlive)
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> WithTag(this IEnumerable<GameEntity> entities, int tag)
        {
            foreach (var entity in entities)
            {
                if (entity.Entity.Tag == tag)
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> Enabled(this IEnumerable<GameEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Enabled && entity.Entity.Enabled)
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> Excluding(this IEnumerable<GameEntity> entities, GameEntity exclude)
        {
            foreach (var entity in entities)
            {
                if (entity != exclude)
                    yield return entity;
            }
        }

        public static IEnumerable<GameEntity> Excluding(this IEnumerable<GameEntity> entities, HashSet<GameEntity> exclude)
        {
            foreach (var entity in entities)
            {
                if (!exclude.Contains(entity))
                    yield return entity;
            }
        }

        public static GameEntity ClosestTo(this IEnumerable<GameEntity> entities, Vector2 position)
        {
            GameEntity closest = null;
            float closestDistSq = float.MaxValue;

            foreach (var entity in entities)
            {
                float distSq = Vector2.DistanceSquared(entity.Transform.Position, position);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = entity;
                }
            }

            return closest;
        }
    }
}