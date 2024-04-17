using ImGuiNET;
using Nez.ImGuiTools.TypeInspectors;
using Nez.ImGuiTools.Windows;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeFNA.Items;
using System.IO;
using System.Reflection;
using Nez.Persistence;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeFNA
{
    public class CreateItemWindow : Window
    {
        public override string Title => "Create Item";

        List<List<AbstractTypeInspector>> _effectInspectors = new List<List<AbstractTypeInspector>>();
        List<Type> _effectTypes;
        int _effectTypeSelected;
        int _removeIndex;
        TextureInspector _texInspector;
        BitmaskInspector _bitmaskInspector;

        SerializedItem _item = new();
        Texture2D _itemTex;

        public CreateItemWindow()
        {
            _effectTypes = ReflectionUtils.GetAllSubclasses(typeof(ItemEffect), true);

            _texInspector = new TextureInspector();
            _texInspector.SetTarget(this,
                GetType().GetField(nameof(_itemTex), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            _texInspector.Initialize();
            _texInspector.OnTexturePicked += path => _item.TexturePath = path;

            _bitmaskInspector = new BitmaskInspector();
            _bitmaskInspector.SetTarget(
                _item,
                _item.GetType().GetField(
                    nameof(_item.ItemPoolMask), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            ));
            _bitmaskInspector.Initialize();
        }

        public override void Show()
        {
            _removeIndex = -1;
            ImGui.InputText("Item ID", ref _item.ItemId, 200);

            _bitmaskInspector.Draw();
            _texInspector.Draw();

            if (ImGui.CollapsingHeader("Effects"))
            {
                ImGui.Indent();

                ImGui.Combo("Effect", ref _effectTypeSelected, _effectTypes.Select(e => e.Name).ToArray(), _effectTypes.Count);
                if(ImGui.Button("Add Selected Effect"))
                {
                    _itemTex = Core.Scene.Content.LoadTexture(ContentPath.Textures.Debug_circle_png);
                    var effect = (ItemEffect)Activator.CreateInstance(_effectTypes[_effectTypeSelected]);
                    _item.Effects.Add(effect);
                    var fieldsToInspect = TypeInspectorUtils.GetAllFieldsWithAttribute<InspectableAttribute>(effect.GetType());
                    var inspectors = new List<AbstractTypeInspector>();
                    
                    foreach (var field in fieldsToInspect)
                    {
                        var inspector = TypeInspectorUtils.GetInspectorForType(field.FieldType, effect, field);
                        if(inspector != null)
                        {
                            inspectors.Add(inspector);
                            inspector.SetTarget(effect, field);
                            inspector.Initialize();
                        }
                    }
                    _effectInspectors.Add(inspectors);
                }

                for(int i = 0; i < _item.Effects.Count; i++)
                {
                    var effect = _item.Effects[i];
                    if (ImGui.CollapsingHeader(effect.GetType().Name))
                    {
                        ImGui.Indent();

                        if(ImGui.Button("Remove effect")) _removeIndex = i;

                        foreach (var inspector in _effectInspectors[i]) inspector.Draw();

                        ImGui.Unindent();
                    }
                }

                ImGui.Unindent();
            }
            
            if(_removeIndex > -1 && _removeIndex < _item.Effects.Count)
            {
                _item.Effects.RemoveAt(_removeIndex);
                _effectInspectors.RemoveAt(_removeIndex);
            }

            if(ImGui.Button("Clear Values"))
            {
                _item.ItemId = string.Empty;
                _item.TexturePath = string.Empty;
                _item.Effects.Clear();
                _effectInspectors.Clear();
            }

            if (ImGui.Button("Save Item"))
            {
                var savePath = $"./{_item.ItemId}.item";
                bool isValid = _item.TexturePath != string.Empty && _item.ItemId != string.Empty;
                Insist.IsTrue(isValid, "Please fill at least ItemID and Texture Path");

                if (isValid)
                {
                    Insist.IsTrue(
                        File.Exists(_item.TexturePath),
                        $"Couldn't find item texture at '{_item.TexturePath}' when saving item '{_item.ItemId}'."
                    );

                    Directory.CreateDirectory(Directory.GetParent(savePath).FullName);
                    File.WriteAllText(savePath, NsonEncoder.ToNson(_item, new NsonSettings()));
                    System.Diagnostics.Process.Start(Directory.GetParent(savePath).FullName);
                }
            }
        }
    }
}
