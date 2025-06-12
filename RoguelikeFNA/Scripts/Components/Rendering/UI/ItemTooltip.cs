using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tweens;
using Nez.UI;


namespace RoguelikeFNA.UI
{
    public class ItemTooltip : Component, IUpdatable
    {
        [Inspectable] Table _tooltipTable;
        UICanvas _canvas;
        Label _titleLabel;
        Label _descriptionLabel;
        Stage Stage => _canvas.Stage;

        Item _focused;
        // ITween<float> _focusTween;
        // ITween<float> _unfocusTween;
        float _focusTime = 0;

        const float FOCUS_TIME = 0.1f;
        // const float TWEEN_DURATION = 0.15f;
        const float SCREEN_FILL_AMOUNT = 0.6f;
        const float SCREEN_PAD_TOP = 50f;
        bool IUpdatable.UpdateOnPause { get => false; set {} }

        float TooltipTargetWidth()
        {
            return Stage.GetWidth() * SCREEN_FILL_AMOUNT;
        }

        void RepositionTable()
        {
            _tooltipTable.SetWidth(TooltipTargetWidth());
            _tooltipTable.x = Stage.GetWidth() / 2 - TooltipTargetWidth() / 2;
            _tooltipTable.y = SCREEN_PAD_TOP;
        }

        void InAnimation()
        {
            _canvas.Enabled = true;

            // if (_unfocusTween.IsRunning())
            //     _unfocusTween.Stop();

            // _focusTween.Start();
            _titleLabel.SetText(_focused.Name);
            _descriptionLabel.SetText(_focused.Description);
        }

        void OutAnimation()
        {
            // if (_focusTween.IsRunning())
            //     _focusTween.Stop();

            // _unfocusTween.Start();
            _titleLabel.SetText("");
            _descriptionLabel.SetText("");
            // yield return _unfocusTween.WaitForCompletion();
            _canvas.Enabled = false;
        }

        public override void OnAddedToEntity()
        {
            Item.OnHoverAction += OnItemHover;

            _canvas = Entity.AddComponent(new UICanvas());
            _canvas.IsFullScreen = true;

            _tooltipTable = new Table();
            // _focusTween = _tooltipTable.Tween("width", TooltipTargetWidth(), TWEEN_DURATION).SetRecycleTween(false);
            // _unfocusTween = _tooltipTable.Tween("width", 0f, TWEEN_DURATION).SetRecycleTween(false);
            _tooltipTable.SetFillParent(false).Top();
            _tooltipTable.Defaults().SetPadTop(10).SetPadBottom(10).SetPadLeft(20).SetPadRight(20);
            _tooltipTable.SetHeight(150);
            _tooltipTable.SetBackground(new PrimitiveDrawable(Color.Black));

            _titleLabel = new Label("Sample Text");
            _tooltipTable.Add(_titleLabel)
                .SetPadTop(20)
                .SetPadBottom(20)
                .GetElement<Label>()
                .SetFontScale(4);
            _tooltipTable.Row();

            _descriptionLabel = new Label("Sample Description");
            _tooltipTable.Add(_descriptionLabel)
                .GetElement<Label>()
                .SetFontScale(2);

            Stage.AddElement(_tooltipTable);
            RepositionTable();
            _canvas.Enabled = false;
        }

        public override void OnRemovedFromEntity()
        {
            Item.OnHoverAction -= OnItemHover;
        }

        void IUpdatable.Update()
        {
            _focusTime -= Time.DeltaTime;
            if (_focusTime < 0 && _focused != null)
            {
                // Core.StartCoroutine(OutAnimation());
                OutAnimation();
                _focused = null;
            }
        }

        void OnItemHover(Item item, Entity source)
        {
            if (_focused == null)
            {
                _focused = item;
                InAnimation();
            }
            _focusTime = FOCUS_TIME;
        }
    }
}