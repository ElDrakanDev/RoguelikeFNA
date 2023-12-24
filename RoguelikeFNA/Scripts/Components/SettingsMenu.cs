using Nez;
using Nez.UI;
using Microsoft.Xna.Framework;
using System;

namespace RoguelikeFNA
{
    public class SettingsMenu : UICanvas
    {
        ConfigManager _configManager;

        Table _container;
        Table _mainSection;
        Table _audioSection;
        Table _videoSection;

        Table _activeSection;
        TextButtonStyle _textButtonStyle;
        CheckBoxStyle _checkBoxStyle;
        SliderStyle _sliderStyle;
        SelectBoxStyle _selectBoxStyle;

        public Action OnClosed;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _configManager = Core.GetGlobalManager<ConfigManager>();

            _container = Stage.AddElement(new Table())
                .SetFillParent(true);

            SetStyles();
            CreateSections();

            PopulateMainSection();
            PopulateVideoSection();
            PopulateAudioSection();

            SwitchToSection(_mainSection);
        }

        void SetStyles()
        {
            _textButtonStyle = TextButtonStyle.Create(
                Color.Gray, Color.Black, Color.Black
            );
            _textButtonStyle.FontScale = 2;
            _sliderStyle = SliderStyle.Create(Color.White, Color.Gray);

            _checkBoxStyle = new CheckBoxStyle(
                new PrimitiveDrawable(30, 30, Color.White),
                new PrimitiveDrawable(30, 30, Color.Yellow),
                null,
                Color.White
            )
            {
                FontScale = 2
            };

            var whiteDrawable = new PrimitiveDrawable(Color.White, 6, 4);
            var grayDrawable = new PrimitiveDrawable(Color.Gray, 6, 4);
            var yellowDrawable = new PrimitiveDrawable(Color.PaleGoldenrod, 6, 4);
            _selectBoxStyle = new SelectBoxStyle(
                Graphics.Instance.BitmapFont,
                Color.Black,
                whiteDrawable,
                new ScrollPaneStyle(whiteDrawable, grayDrawable, grayDrawable, grayDrawable, grayDrawable),
                new ListBoxStyle(Graphics.Instance.BitmapFont, Color.Black, Color.Black, yellowDrawable)
            );
        }
        void CreateSections()
        {
            _mainSection = new Table()
                .SetFillParent(true);
            _audioSection = new Table()
                .SetFillParent(true);
            _videoSection = new Table()
                .SetFillParent(true);
        }

        void PopulateAudioSection()
        {
            AddSlider(_audioSection, TranslationManager.GetTranslation("master_volume"), _configManager.Config.MasterVolume)
                .OnChanged += val => _configManager.Config.MasterVolume = val;
            AddSlider(_audioSection, TranslationManager.GetTranslation("bgm"), _configManager.Config.BackgroundMusicVolume)
                .OnChanged += val => _configManager.Config.BackgroundMusicVolume = val;
            AddSlider(_audioSection, TranslationManager.GetTranslation("sfx"), _configManager.Config.SoundEffectVolume)
                .OnChanged += val => _configManager.Config.SoundEffectVolume = val;
            AddButton(_audioSection, TranslationManager.GetTranslation("back"))
                .OnClicked += evt => SwitchToSection(_mainSection);
        }

        void PopulateMainSection()
        {
            AddButton(_mainSection, TranslationManager.GetTranslation("video"))
                .OnClicked += evt => SwitchToSection(_videoSection);
            AddButton(_mainSection, TranslationManager.GetTranslation("audio"))
                .OnClicked += evt => SwitchToSection(_audioSection);
            AddButton(_mainSection, TranslationManager.GetTranslation("back"))
                .OnClicked += evt => {
                    OnClosed?.Invoke();
                    _configManager.ApplyChanges();
                    Enabled = false;
                };
        }

        void PopulateVideoSection()
        {
            var fullscreenCheck = AddCheckBox(_videoSection, TranslationManager.GetTranslation("fullscreen"));
            fullscreenCheck.IsChecked = _configManager.Config.Fullscreen;

            fullscreenCheck.OnClicked += btn => {
                _configManager.Config.Fullscreen = !_configManager.Config.Fullscreen;
                _configManager.ApplyChanges();
            };
            var resolutionSelection = new SelectBox<ScreenResolution>(_selectBoxStyle);
            var resolutions = ScreenResolution.GetAvailableResolutions();
            resolutionSelection.SetItems(resolutions);
            resolutionSelection.SetSelected(resolutions.Find(res => res == ScreenResolution.Current()));
            resolutionSelection.OnChanged += resolution =>
            {
                _configManager.Config.ScreenResolution = resolution;
                _configManager.ApplyChanges();
            };
            _videoSection.Add(GroupElementWithLabel(TranslationManager.GetTranslation("resolution"), resolutionSelection));
            _videoSection.Row();
            AddButton(_videoSection, TranslationManager.GetTranslation("back"))
                .OnClicked += evt => SwitchToSection(_mainSection);
        }

        #region Helpers
        void SwitchToSection(Table section)
        {
            if (_activeSection != null && _activeSection != section)
                _container.RemoveElement(_activeSection);
            _activeSection = _container.AddElement(section);
        }

        TextButton AddButton(Table table, string text)
        {
            var newBtn = new TextButton(text, _textButtonStyle);
            table.Add(newBtn)
                .SetMinWidth(150)
                .SetMinHeight(40)
                .Pad(25);
            table.Row();
            return newBtn;
        }

        Slider AddSlider(Table table, string label, float startValue = 0)
        {
            var newSlider = new Slider(0, 1, 0.05f, false, _sliderStyle);
            newSlider.Value = startValue;
            var row = new Table()
                .PadBottom(20);

            row.Add(new Label(label, new LabelStyle() { FontScale = 2 }))
                .SetMinWidth(300);
            row.Add(newSlider).SetPadLeft(50);

            table.Add(row);
            table.Row();
            return newSlider;
        }

        CheckBox AddCheckBox(Table table, string label)
        {
            var checkBox = new CheckBox(label, _checkBoxStyle);
            table.Add(checkBox)
                .SetPadBottom(20);
            table.Row();
            return checkBox;
        }

        Table GroupElementWithLabel(string label, Element element)
        {
            var row = new Table()
                .PadBottom(20);

            row.Add(new Label(label, new LabelStyle() { FontScale = 2 }))
                .SetMinWidth(300);
            row.Add(element).SetPadLeft(50);
            return row;
        }
        #endregion
    }
}
