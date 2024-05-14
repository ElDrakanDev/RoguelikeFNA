using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Nez;
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

        public Vector2 Offset;
        public Vector2 Scale;
        public float Rotation;

        public List<Component> Components;

        public List<SerializedEntity> Children;

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
        public Entity ToEntity(Entity parent = null)
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
            
            if(parent != null)
            {
                entity.SetParent(parent);
                entity.LocalPosition = Offset;
            }
            entity.LocalRotation = Rotation;
            entity.LocalScale = Scale;
            entity.Enabled = Enabled;
            entity.Name = Name;
            entity.Tag = Tag;

            foreach(var component in Components)
               entity.AddComponent(component.Clone());

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

        public static SerializedEntity ConvertToSerialized(Entity entity)
        {
            var serialized = new SerializedEntity()
            {
                Name = entity.Name,
                TypeName = string.Empty,
                Enabled = entity.Enabled,
                Tag = entity.Tag,
                Scale = entity.LocalScale,
                Rotation = entity.LocalRotation,
                Children = new List<SerializedEntity>(),
                Components = new List<Component>()
            };
            if (entity.GetType().IsSubclassOf(typeof(Entity)))
                serialized.TypeName = entity.GetType().FullName;

            if (entity.Transform.Parent != null)
                serialized.Offset = entity.LocalPosition;

            if(entity.ChildCount > 0)
            {
                for (int i = 0; i < entity.ChildCount; i++)
                {
                    var child = entity.Transform.GetChild(i).Entity;
                    serialized.Children.Add(ConvertToSerialized(child));
                }
            }

            for (int i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i];
                if (component.GetType().GetAttribute<SerializableAttribute>() != null)
                    serialized.Components.Add(component.Clone());
            }

            return serialized;
        }
    }
}
