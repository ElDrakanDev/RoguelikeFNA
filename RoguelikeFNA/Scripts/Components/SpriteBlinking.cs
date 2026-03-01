using System;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace RoguelikeFNA
{
    [Serializable]
    public class SpriteBlinking : Component, IUpdatable
    {
        public bool IsActive;
        public float BlinkInterval = 0.08f;
        public byte VisibleAlpha = byte.MaxValue;
        public byte HiddenAlpha = 0;

        SpriteRenderer _renderer;
        Color _normalColor;
        float _blinkTimer;
        bool _isHidden;

        public SpriteBlinking() { }

        public SpriteBlinking(SpriteRenderer renderer)
        {
            _renderer = renderer;
        }

        public override void OnAddedToEntity()
        {
            if (_renderer is null)
                SetRenderer(Entity.GetComponent<SpriteRenderer>());
        }

        public override void OnRemovedFromEntity()
        {
            RestoreColor();
        }

        public void SetRenderer(SpriteRenderer renderer)
        {
            _renderer = renderer;
            if (_renderer is null)
                return;

            _normalColor = _renderer.Color;
            SetAlpha(VisibleAlpha);
        }

        public void Update()
        {
            if (_renderer is null)
                return;

            if (!IsActive)
            {
                if (_isHidden)
                {
                    RestoreColor();
                    _isHidden = false;
                }

                _blinkTimer = 0;
                _normalColor = _renderer.Color;
                return;
            }

            _blinkTimer -= Time.DeltaTime;
            if (_blinkTimer > 0)
                return;

            _blinkTimer = BlinkInterval;
            _isHidden = !_isHidden;
            SetAlpha(_isHidden ? HiddenAlpha : VisibleAlpha);
        }

        void SetAlpha(byte alpha)
        {
            var color = _normalColor;
            color.A = alpha;
            _renderer.Color = color;
        }

        void RestoreColor()
        {
            if (_renderer is null)
                return;

            _renderer.Color = _normalColor;
        }
    }
}