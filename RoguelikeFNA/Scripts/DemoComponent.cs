using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.Persistence;
using System.IO;

namespace RoguelikeFNA
{
    public class DemoComponent : Component
    {
        HitboxHandler hitboxHandler;
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            hitboxHandler = Entity.AddComponent(new HitboxHandler());
            hitboxHandler.OnCollisionEnter += col => Debug.Log($"Collided with {col}");
            Core.GetGlobalManager<ImGuiManager>()?.RegisterDrawCommand(ImguiDraw);
        }

        void ImguiDraw()
        {
            if(ImGui.Begin("Hitbox Handler"))
                if (ImGui.Button("Clear Collisions"))
                    hitboxHandler.ClearCollisions();
        }
    }
}
