using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoguelikeFNA
{
    public static class TiledMapRendererExt
    {
        static Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();

        public static TiledEntity GetTiledEntity(this TiledMapRenderer map, int id) {
            foreach (var child in map.Entity.Children())
            {
                var tiledEnt = child as TiledEntity;
                if (tiledEnt is not null && tiledEnt.TiledId == id)
                    return tiledEnt;
            }
            Debug.Warn("No entities found with id {0} on map {1}.", id, map.Entity.Name);
            return null;
        }

        public static void CreateObjects(this TiledMapRenderer map)
        {
            var entities = map.TiledMap.GetObjectGroup("Entities");
            if (entities == null)
                return;

            List<Tuple<FieldInfo, object, int>> tiledEntityFields = new();

            foreach (var obj in entities.Objects)
            {
                var entity = new TiledEntity(obj.Name, obj.Id, obj.Width, obj.Height);
                map.Entity.Scene.AddEntity(entity)
                    .SetLocalPosition(new Vector2(obj.X, obj.Y))
                    .SetLocalRotation(Mathf.Deg2Rad * obj.Rotation)
                    .Transform.SetParent(map.Transform);

                if (obj.Properties is null)
                    continue;

                SetEntityFields(obj, entity);

                if (!obj.Properties.TryGetValue("InternalType", out string typeName))
                    continue;

                if (_typeDict.TryGetValue(typeName, out Type type) is false)
                {
                    try
                    {
                        type = Type.GetType(typeName, true);
                        _typeDict[typeName] = type;
                    }
                    catch (Exception ex)
                    {
                        Debug.Error(ex.ToString());
                        continue;
                    }
                }
                var prefabComponent = Activator.CreateInstance(type) as IPrefab;
                var fields = prefabComponent.GetType().GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                foreach (var field in fields)
                {
                    if (obj.Properties.TryGetValue(field.Name, out string value))
                    {
                        SetFieldValue(prefabComponent, field, value, tiledEntityFields);
                    }
                }

                var component = (Component)prefabComponent;
                component.Entity = entity;
                prefabComponent.LoadPrefab();
                entity.AddComponent(component);
            }

            foreach (var item in tiledEntityFields)
            {
                item.Deconstruct(out var field, out var obj, out var id);
                field.SetValue(obj, map.GetTiledEntity(id));
            }
        }

        static void SetFieldValue(object obj, FieldInfo field, string value, List<Tuple<FieldInfo, object, int>> tiledEntityFields)
        {
            if (field.FieldType == typeof(int))
                field.SetValue(obj, Convert.ToInt32(value));
            else if (field.FieldType == typeof(float))
                field.SetValue(obj, float.Parse(value));
            else if (field.FieldType == typeof(string))
                field.SetValue(obj, value);
            else if (field.FieldType == typeof(bool))
                field.SetValue(obj, bool.Parse(value));
            else if (field.FieldType.IsEnum)
                field.SetValue(obj, Enum.Parse(field.FieldType, value));
            else if (field.FieldType == typeof(TiledEntity))
                // Register entity if to fill
                tiledEntityFields.Add(new(field, obj, Convert.ToInt32(value)));
            else
                throw new NotImplementedException();
        }

        static void SetEntityFields(TmxObject obj, TiledEntity entity)
        {
            entity.Name = obj.Name;
            if (obj.Properties.TryGetValue("Tag", out var tag))
                entity.Tag = (int)Enum.Parse(typeof(Tag), tag);
        }
    }
}
