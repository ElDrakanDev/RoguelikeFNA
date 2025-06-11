using Nez;
using Nez.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Particles;

namespace RoguelikeFNA.UI
{
    public class MainMenu : UICanvas
    {
        Table _table;
        TextButtonStyle _textButtonStyle;
        SettingsMenu _settingsMenu;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            TranslationManager.OnLanguageChanged += CreateElements;
            CreateElements();
        }

        public override void OnRemovedFromEntity() => TranslationManager.OnLanguageChanged -= CreateElements;

        void CreateElements()
        {
            _settingsMenu = Entity.AddComponent(new SettingsMenu());
            _settingsMenu.Enabled = false;
            _settingsMenu.OnClosed += () => SetEnabled(true);

            _textButtonStyle = TextButtonStyle.Create(
                Color.Gray, Color.Black, Color.Black
            );
            _textButtonStyle.FontScale = 2;

            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);
            _table.SetBackground(null);

            var logo = Entity.Scene.Content.LoadTexture(ContentPath.Sprites.Nezlogoblack_png);
            _table.Add(new Image(logo))
                .Pad(50);
            _table.Row();

            AddButton(TranslationManager.GetTranslation("new_game"))
                .OnClicked += evt => Core.Scene = new LevelScene();
            var continueBtn = AddButton(TranslationManager.GetTranslation("continue"));
            AddButton(TranslationManager.GetTranslation("settings"))
                .OnClicked += evt => { _settingsMenu.SetEnabled(true); SetEnabled(false); };
            AddButton(TranslationManager.GetTranslation("exit"))
                .OnClicked += evt => Core.Exit();

            Stage.SetGamepadFocusElement(continueBtn);
        }

        TextButton AddButton(string text)
        {
            var newBtn = new TextButton(text, _textButtonStyle);
            _table.Add(newBtn)
                .SetMinWidth(150)
                .SetMinHeight(40)
                .Pad(25);
            _table.Row();
            return newBtn;
        }
    }
}
