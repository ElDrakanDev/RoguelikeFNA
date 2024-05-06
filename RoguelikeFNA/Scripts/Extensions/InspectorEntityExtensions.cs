using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Nez.ImGuiTools.ObjectInspectors;
using System.Reflection;
using Nez.ImGuiTools;
using Nez;

namespace RoguelikeFNA.Scripts.Extensions
{
    public class InspectorEntityExtensions
    {
        static Type[] _prefabTypes;
        static Type _activePrefabType;

        static Type[] GetPrefabTypes()
        {
            if(_prefabTypes is null){
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                List<Type> types = new List<Type>();
                foreach (var assembly in assemblies)
                {
                    types.AddRange(
                        assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPrefab)))
                    );
                }
                _prefabTypes = types.ToArray();
            }
            return _prefabTypes;
        }

        [InspectorEntityListExtension]
        static void DrawPrefabCreator()
        {
            var popupName = "create-prefab";
            if (NezImGui.CenteredButton("Instantiate Prefab", 0.6f))
                ImGui.OpenPopup(popupName);
            if (ImGui.BeginPopup(popupName))
            {
                var prefabs = GetPrefabTypes();
                var names = prefabs.Select(t => t.Name).ToArray();
                
                if(prefabs.Length > 0)
                {
                    int index = 0;
                    if (ImGui.Combo("Select the prefab to instantiate", ref index, names, names.Length))
                    {
                        _activePrefabType = prefabs[index];
                    }

                }
                else
                {
                    ImGui.Text("No prefabs found. Inherit interface IPrefab and add its default values to create one.");
                }

                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGui.GetItemRectSize().X);

                ImGui.PushStyleColor(ImGuiCol.Button, Microsoft.Xna.Framework.Color.Green.PackedValue);
                if (ImGui.Button("Create") && _activePrefabType is not null)
                {

                    var entityName = $"{_activePrefabType.Name} (prefab clone)";
                    var newPrefab = Activator.CreateInstance(_activePrefabType) as IPrefab;
                    var newEntity = new Entity(entityName);
                    newEntity.AddComponent(newPrefab as Component);
                    Core.Scene.AddEntity(newEntity);
                    newPrefab.AddComponents();
                    ImGui.CloseCurrentPopup();
                    _activePrefabType = null;
                }

                ImGui.PopStyleColor();
            }
        }
    }
}
