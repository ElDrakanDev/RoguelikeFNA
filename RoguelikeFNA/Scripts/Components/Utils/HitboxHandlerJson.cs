using System;
using System.Collections.Generic;
using Nez;
using Nez.Persistence;

namespace RoguelikeFNA.Utils
{
    [Serializable]
    public class HitboxHandlerJson : Component
    {
        public string HitboxJsonPath;
        public int PhysicsLayer;
        public int HitboxLayers;

        public override void OnAddedToEntity()
        {
            var hitboxes = Entity.Scene.Content.LoadJson<Dictionary<string, List<HitboxGroup>>>(HitboxJsonPath);
            var handler = Entity.GetOrCreateComponent<HitboxHandler>();
            handler.AnimationsHitboxes = hitboxes;
            handler.PhysicsLayer = PhysicsLayer;
            handler.HitboxLayers = HitboxLayers;
        }
    }
}