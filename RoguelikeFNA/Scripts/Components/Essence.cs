using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace RoguelikeFNA
{
    public class Essence : Component, IPerishable
    {
        class EssenceParticle : SpriteRenderer, IUpdatable, IPerishable, ITriggerListener
        {
            // TODO: Polish and add essence pickup events
            public int Value;
            Vector2 _velocity;
            bool _startedChase = false;
            public bool UpdateOnPause { get => false; set { } }
            const float VEL_VARIANCE = 5f;
            const float SPEED = 30f;
            const float CHASE_THRESHOLD = 0.2f;
            const float DECELERATION = 0.02f;
            Entity _target;
            Mover _mover;
            TrailRibbon _trail;

            public EssenceParticle(int value, Entity target) : base()
            {
                Value = value;
                _target = target;
            }

            public override void OnAddedToEntity()
            {
                base.OnAddedToEntity();
                Entity.AddComponent(new BoxCollider() { IsTrigger = true });
                Entity.Scene.GetSceneComponent<LevelNavigator>().OnRoomChanged += AddEssence;
                _mover = Entity.AddComponent(new Mover());
                _trail = Entity.Scene.AddEntity(new()).SetParent(Entity).AddComponent(new TrailRibbon());
                _trail.StartColor = new Color(250, 214, 255, 0.75f);
                _trail.EndColor = new Color(255, 255, 255, 0);
                _trail.RibbonRadius = 0.5f;
                _velocity = new Vector2(Random.Range(-VEL_VARIANCE, VEL_VARIANCE), Random.Range(-VEL_VARIANCE, VEL_VARIANCE));
            }

            public void Update()
            {
                if (_target is null || !_target.Enabled)
                {
                    Entity.Scene.GetSceneComponent<EssenceSceneComponent>().AddEssence(Value);
                    Entity.Destroy();
                }
                else
                {
                    if (!_startedChase && _velocity.Magnitude() > CHASE_THRESHOLD)
                    {
                        _velocity *= Mathf.Pow(DECELERATION, Time.DeltaTime);
                    }
                    else
                    {
                        _startedChase = true;
                        _velocity = (_target.Position - Entity.Position).Normalized() * (_velocity.Magnitude() + SPEED * Time.DeltaTime);
                    }
                    _mover.Move(_velocity, out var _);
                }
            }

            public void OnTriggerEnter(Collider other, Collider local)
            {
                if (other.Entity.Tag.IsFlagSet((int)Tag.Player))
                {
                    AddEssence(null);
                }
            }

            public void OnTriggerExit(Collider other, Collider local) { }

            public override void OnRemovedFromEntity()
            {
                base.OnRemovedFromEntity();
                Entity.Scene.GetSceneComponent<LevelNavigator>().OnRoomChanged -= AddEssence;
            }

            void AddEssence(Entity _)
            {
                _trail.Enabled = false;
                Entity.Scene.GetSceneComponent<EssenceSceneComponent>().AddEssence(Value);
                Entity.Destroy();
            }
        }

        public int TotalValue;
        List<EssenceParticle> _particles;

        const int SMALL = 5;
        const int MEDIUM = 25;
        const int LARGE = 100;

        public static Dictionary<int, string> SpriteBreakpoints = new()
        {
            {SMALL, ContentPath.Sprites.Essence.Small_png},
            {MEDIUM, ContentPath.Sprites.Essence.Medium_png},
            {LARGE, ContentPath.Sprites.Essence.Large_png}
        };

        static int[] _orderedBreakpoints;

        public Essence(int totalValue)
        {
            TotalValue = totalValue;
            _particles = ListPool<EssenceParticle>.Obtain();
        }

        int[] GetOrderedBreakpoints()
        {
            if (_orderedBreakpoints is null)
                _orderedBreakpoints = SpriteBreakpoints.Keys.OrderByDescending(x => x).ToArray();
            return _orderedBreakpoints;
        }

        void CreateParticle(int value, Entity target, string spritePath)
        {
            var entity = new Entity();
            entity.SetParent(Entity);
            var particle = (EssenceParticle)new EssenceParticle(value, target)
                .SetTexture(Entity.Scene.Content.LoadTexture(spritePath));
            entity.AddComponent(particle);
            _particles.Add(particle);
            Entity.Scene.AddEntity(entity);
        }

        public static void Create(Vector2 position, int amount)
        {
            var entity = new Entity().SetPosition(position);
            entity.AddComponent(new Essence(amount));
            Core.Scene.AddEntity(entity);
        }

        public override void OnAddedToEntity()
        {
            var particleBreakpoints = GetOrderedBreakpoints();

            var target = Entity.Scene.FindEntitiesWithTag((int)Tag.Player).Enabled().Closest(Transform.Position);

            foreach (var breakpoint in particleBreakpoints)
            {
                int quantity = TotalValue / breakpoint;
                TotalValue -= quantity * breakpoint;

                for (int i = 0; i < quantity; i++)
                    CreateParticle(breakpoint, target, SpriteBreakpoints[breakpoint]);
            }
            if (_particles.Count == 0)
                CreateParticle(TotalValue, target, SpriteBreakpoints[SMALL]);
            else
                _particles[0].Value += TotalValue;
        }

        public override void OnRemovedFromEntity()
        {
            _particles.Clear();
            ListPool<EssenceParticle>.Free(_particles);
        }
    }
}