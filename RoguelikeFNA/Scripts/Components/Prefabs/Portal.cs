using Nez;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeFNA.Prefabs
{
    public class Portal : Component, IInteractListener, IPrefab
    {
        SpriteAnimator _anim;

        public void AddComponents()
        {
            _anim = Entity.AddComponent(new SpriteAnimator())
                .AddAnimationsFromAtlas(Entity.Scene.Content.LoadSpriteAtlas(ContentPath.Atlases.Portal.Portal_atlas));
            _anim.Play(_anim.Animations.Keys.First());
            Entity.AddComponent(new InteractableOutline());
        }

        public void OnHover(Entity source)
        {

        }

        public void OnInteract(Entity source)
        {
            Entity.Scene.GetSceneComponent<LevelNavigator>().Move(1);
        }
    }
}
