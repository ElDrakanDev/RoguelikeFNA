using Nez;

namespace RoguelikeFNA
{
    public class BaseScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(1280, 720, Scene.SceneResolutionPolicy.BestFit);
        }
    }
}
