using ImGuiNET;
using Nez;
using Nez.ImGuiTools;
using Nez.Persistence;
using System.IO;

namespace RoguelikeFNA
{
    [System.Serializable]
    public class ExampleData : Component
    {
        public int Number;
        public string Text;
    }

    public class DemoComponent : Component
    {
        public ExampleData exampleData;
        const string PATH = "./example_data.data";

        public override void OnAddedToEntity()
        {
            Core.GetGlobalManager<ImGuiManager>().RegisterDrawCommand(ImguiDraw);
            exampleData = Entity.AddComponent<ExampleData>();
        }

        void ImguiDraw()
        {
            if(ImGui.Button("Export data"))
            {
                var nson = Nson.ToNson(exampleData, true);
                using(StreamWriter writer = new StreamWriter(PATH))
                {
                    writer.Write(nson);
                }
            }
            if(ImGui.Button("Import data"))
            {
                var nson = File.ReadAllText(PATH);
                exampleData = (ExampleData)Nson.FromNson(nson);
            }

        }
    }
}
