using Microsoft.Xna.Framework;
using Nez;
using Nez.Tweens;
using RoguelikeFNA.Generation;

namespace RoguelikeFNA.LevelElements
{
    public class Portal : Component, IInteractListener
    {
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
