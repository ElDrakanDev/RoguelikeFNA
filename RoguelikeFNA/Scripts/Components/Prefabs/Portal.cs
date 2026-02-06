using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Tweens;
using System.Linq;
using RoguelikeFNA.Generation;

namespace RoguelikeFNA.Prefabs
{
    public class Portal : Component, IInteractListener, IPrefab
    {
        SpriteAnimator _anim;
        public bool IsExit;
        const float ENTRANCE_TWEEN_TIME = 1f;
        ITween<Vector2> _tween;

        public override void OnAddedToEntity()
        {
            if (!IsExit)
                _tween = Transform.TweenLocalScaleTo(0, ENTRANCE_TWEEN_TIME);
        }

        public override void OnEnabled()
        {
            if (!IsExit)
            {
                Transform.LocalScale = Vector2.One;
                _tween.Start();
            }
        }

        public void LoadPrefab()
        {
            _anim = Entity.AddComponent(new SpriteAnimator())
                .AddAnimationsFromAtlas(Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Portal.Portal_atlas));
            _anim.Play(_anim.Animations.Keys.First());
            if (IsExit)
                Entity.AddComponent(new InteractableOutline());
        }

        public void OnHover(Entity source)
        {

        }

        public void OnInteract(Entity source)
        {
            if(IsExit)
                Entity.Scene.GetSceneComponent<LevelNavigator>().Move(1);
        }
    }
}
