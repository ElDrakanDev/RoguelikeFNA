using Nez;

namespace RoguelikeFNA.Rendering
{
    public class TiledCamera : FollowCamera
    {
        public TiledCamera(Entity follow) : base(follow)
        {
            MapLockEnabled = true;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            UpdateMapSize(Entity.Scene.GetSceneComponent<LevelNavigator>().ActiveTiledMap);
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Entity.Scene.GetSceneComponent<LevelNavigator>().OnRoomChanged += UpdateMapSize;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Entity.Scene.GetSceneComponent<LevelNavigator>().OnRoomChanged -= UpdateMapSize;
        }

        void UpdateMapSize(Entity tmxEntity) {
            MapSize = tmxEntity.GetComponent<TiledMapRenderer>().Bounds.Size;
        }
    }
}