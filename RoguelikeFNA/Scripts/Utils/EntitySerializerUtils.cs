using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.ImGuiTools.ObjectInspectors;
using Nez.Persistence;

namespace RoguelikeFNA.Utils
{
    [Serializable]
    public class SerializedEntity
    {
        static Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public string Name;
        public bool Enabled;
        public int Tag;
        public string TypeName;

        public Vector2 Scale;

        public List<Component> Components;

        public List<SerializedChild> Children;

        public Entity ToEntity(Entity parent)
        {
            Entity entity;
            if (TypeName != string.Empty)
            {
                if (_types.TryGetValue(TypeName, out Type type) is false)
                {
                    type = Type.GetType(TypeName, true);
                    _types[TypeName] = type;
                }
                entity = (Entity)Activator.CreateInstance(type);
            }
            else
                entity = new Entity();
            return SetEntityValues(entity, parent);
        }

        /// <summary>
        /// Converts the SerializedEntity to Entity and adds it to the Scene. Will also add its children.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="parent"></param>
        public Entity AddToScene(Scene scene, Entity parent = null)
        {
            var entity = ToEntity(parent);
            scene.AddEntity(entity);
            foreach(var child in Children)
                child.AddToScene(scene, entity);
            return entity;
        }

        /// <summary>
        /// Converts the SerializedEntity to an Entity. Does not create its children
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual Entity SetEntityValues(Entity self, Entity parent = null)
        {            
            self.LocalScale = Scale;
            self.Enabled = Enabled;
            self.Name = Name;
            self.Tag = Tag;

            foreach(var component in Components)
               self.AddComponent(component.Clone());

            return self;
        }
    }

    [Serializable]
    public class SerializedChild : SerializedEntity
    {
        public Vector2 Offset;
        public float Rotation;

        override public Entity SetEntityValues(Entity self, Entity parent = null)
        {
            var entity = base.SetEntityValues(self, parent);
            if(parent != null)
            {
                entity.SetParent(parent);
                entity.LocalPosition = Offset;
            }
            entity.LocalRotation = Rotation;
            return entity;
        }
    }

    public static class EntitySerializerUtils
    {
        [InspectorEntityContextMenu("Serialize Entity", true)]
        public static void SerializeEntity(Entity entity)
        {
            ImGui.Text($"Serialize Entity: {entity.Name}");
            ImGui.NewLine();
            if(ImGui.Button("Save to Nson"))
            {
                var serializable = ConvertToSerialized(entity);
                var path = Path.Combine(Core.Content.RootDirectory, $"{entity.Name}.nson");
                var content = Nson.ToNson(serializable, true);
                File.WriteAllText(path, content);
                Process.Start(Directory.GetParent(path).Name);
                ImGui.CloseCurrentPopup();
            }
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
        }

        [InspectorEntityListExtension]
        static void AddSerializedEntityToScene()
        {
            var popupName = "create-from-serialized-entity";
            if (NezImGui.CenteredButton("Create from Serialized Entity", 0.8f))
                ImGui.OpenPopup(popupName);
            if (ImGui.BeginPopup(popupName))
            {
                var picker = FilePicker.GetFilePicker(popupName, Path.Combine(Environment.CurrentDirectory, "Content"), ".nson");
                picker.DontAllowTraverselBeyondRootFolder = true;

                if (picker.Draw())
                {
                    try
                    {
                        var serialized = Core.Scene.Content.LoadNson<SerializedEntity>(picker.SelectedFile);
                        serialized.AddToScene(Core.Scene)
                            .SetPosition(Core.Scene.Camera.Position);
                        ImGui.CloseCurrentPopup();
                    }
                    catch (Exception ex)
                    {
                        Nez.Debug.Error($"Couldn't create entity from serialized ({picker.SelectedFile}). Error: \n{ex}");
                    }
                }
                else if (picker.WasCanceled)
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
        }

        public static SerializedEntity ConvertToSerialized(Entity entity)
        {
            var serialized = new SerializedEntity()
            {
                Name = entity.Name,
                TypeName = string.Empty,
                Enabled = entity.Enabled,
                Tag = entity.Tag,
                Scale = entity.LocalScale,
                Children = new(),
                Components = new()
            };
            SetEntityTypeName(serialized, entity);
            AddChildrenToSerialized(serialized, entity);
            AddComponentsToSerialized(serialized, entity);

            return serialized;
        }

        static SerializedChild ConvertToSerializedChild(Entity entity)
        {
            var serialized = new SerializedChild()
            {
                Name = entity.Name,
                TypeName = string.Empty,
                Enabled = entity.Enabled,
                Tag = entity.Tag,
                Scale = entity.LocalScale,
                Rotation = entity.LocalRotation,
                Offset = entity.LocalPosition,
                Children = new(),
                Components = new()
            };
            SetEntityTypeName(serialized, entity);
            AddChildrenToSerialized(serialized, entity);
            AddComponentsToSerialized(serialized, entity);

            return serialized;
        }

        static void AddComponentsToSerialized(SerializedEntity serialized, Entity entity)
        {
            for (int i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i];
                if (component.GetType().GetAttribute<SerializableAttribute>() != null)
                    serialized.Components.Add(component.Clone());
            }
        }

        static void AddChildrenToSerialized(SerializedEntity serialized, Entity entity)
        {
            if(entity.ChildCount > 0)
            {
                for (int i = 0; i < entity.ChildCount; i++)
                {
                    var child = entity.Transform.GetChild(i).Entity;
                    serialized.Children.Add(ConvertToSerializedChild(child));
                }
            }
        }

        static void SetEntityTypeName(SerializedEntity serialized, Entity entity)
        {
            if (entity.GetType().IsSubclassOf(typeof(Entity)))
                serialized.TypeName = entity.GetType().FullName;
        }
    }
}
