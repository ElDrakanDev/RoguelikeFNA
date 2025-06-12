using Nez;
using Nez.UI;

namespace RoguelikeFNA
{
    public class EssenceLabel : UICanvas, IUpdatable
    {
        EssenceSceneComponent _essenceMgr;
        Label _label;
        const float VERT_PAD = 12;
        const float HOR_PAD = 16;
        const float SCALE = 2;

        bool IUpdatable.UpdateOnPause { get => false; set {} }

        void IUpdatable.Update()
        {
            _label.SetText($"Essence: {_essenceMgr.Essence}");
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            _essenceMgr = Entity.Scene.GetSceneComponent<EssenceSceneComponent>();
            _label = new Label("");
            Stage.AddElement(_label);
            _label.SetPosition(HOR_PAD, Stage.GetHeight() - VERT_PAD);
            _label.SetFontScale(SCALE);
        }
    }
}