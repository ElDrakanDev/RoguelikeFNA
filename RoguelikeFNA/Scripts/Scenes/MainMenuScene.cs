using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;

namespace RoguelikeFNA
{
	public class MainMenuScene : BaseScene
    {
        public override void Initialize()
        {
            base.Initialize();
            CreateEntity("main-menu").AddComponent(new MainMenu());
        }
    }
}

