using Nez;
using System;
using Microsoft.Xna.Framework;
using Nez.Textures;

namespace RoguelikeFNA
{
    public class ConnectedSpriteRenderer : Component
    {
        Entity _child;
        TiledSpriteRenderer _tRenderer;

        Sprite _sprite;
        public Sprite Sprite { get => _sprite; set => SetSprite(value); }
        public Transform ExpandTowards;

        public ConnectedSpriteRenderer()
        {
            _tRenderer = new TiledSpriteRenderer();
            _child = new Entity();
        }

        public ConnectedSpriteRenderer(Sprite sprite)
        {
            _tRenderer = new TiledSpriteRenderer();
            _child = new Entity();
            Sprite = sprite;
        }

        public override void OnAddedToEntity()
        {
            Transform.Position += Vector2.One * 200;
            Entity.Scene.AddEntity(_child);
            _child.Transform.SetParent(Transform);
            _child.AddComponent(_tRenderer);
            _child.LocalPosition = Vector2.UnitX * _tRenderer.Sprite.SourceRect.Width + Vector2.UnitY * _tRenderer.Sprite.SourceRect.Height * 0.5f;
            _tRenderer.SetOrigin(new Vector2(_child.LocalPosition.X, _child.LocalPosition.Y * 2));
        }

        void SetSprite(Sprite sprite)
        {
            _tRenderer.SetSprite(sprite);
            _child.LocalPosition = Vector2.UnitX * _tRenderer.Sprite.SourceRect.Width + Vector2.UnitY * _tRenderer.Sprite.SourceRect.Height * 0.5f;
            _tRenderer.SetOrigin(new Vector2(_child.LocalPosition.X, _child.LocalPosition.Y * 2));
        }

        public void Update()
        {
            if (ExpandTowards is null) return;

            Vector2 from = Transform.Position;
            Vector2 to = ExpandTowards.Position;
            Vector2 direction = to - from;
            _tRenderer.Width = (int)(Vector2.Distance(to, from) / Math.Abs(Transform.Scale.X));
            Transform.Rotation = Mathf.Atan2(direction.Y, direction.X);
            bool flip = direction.X >= 0;
            _tRenderer.FlipY = !flip;
        }
    }
}
