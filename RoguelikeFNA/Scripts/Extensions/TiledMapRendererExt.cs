using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;
using RoguelikeFNA.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RoguelikeFNA
{
    public static class TiledMapRendererExt
    {
        static Dictionary<string, Type> _typeNames = new();

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

            List<Tuple<TiledEntity, TmxObject>> entitiesToSetup = new();
            Dictionary<int, TiledEntity> idToEntity = new();

            foreach (var obj in entities.Objects)
            {
                var entity = new TiledEntity(obj.Name, obj.Id, obj.Width, obj.Height);
                map.Entity.Scene.AddEntity(entity)
                    .SetParent(map.Entity)
                    .SetLocalPosition(new Vector2(obj.X, obj.Y))
                    .SetLocalRotation(Mathf.Deg2Rad * obj.Rotation);
                entitiesToSetup.Add(new(entity, obj));
                idToEntity[obj.Id] = entity;
            }

            foreach (var item in entitiesToSetup){
                item.Deconstruct(out var entity, out var obj);

                if (obj.Properties is null)
                    continue;

                if(obj.Properties.TryGetValue("Serializable", out var serializablePath))
                {
                    var serialized = Core.Scene.Content.LoadNson<SerializedEntity>(serializablePath);
                    serialized.SetEntityValues(entity, map.Entity);
                }
                
                // Add additional components and potentially override values from serializable
                SetEntityValuesFromProperties(entity, obj.Properties, idToEntity);
                AddComponentsFromProperties(entity, obj.Properties, idToEntity);
            }
        }

        static void AddComponentsFromProperties(TiledEntity entity, Dictionary<string, string> properties, Dictionary<int, TiledEntity> entities)
        {
            foreach (var prop in properties)
            {
                if (prop.Key.Contains(":"))
                {
                    var parts = prop.Key.Split(':');
                    var compName = parts[0];
                    var compProp = parts[1];

                    if (!_typeNames.TryGetValue(compName, out var type))
                    {
                        try
                        {
                            type = Type.GetType(compName, true);
                            _typeNames[compName] = type;
                        }
                        catch (Exception ex)
                        {
                            Debug.Error(ex.ToString());
                            continue;
                        }
                    }                    

                    if (_typeNames.TryGetValue(compName, out type))
                    {
                        var comp = entity.GetOrCreateComponent(type, false);
                        TrySetComponentValue(comp, type, compProp, prop.Value, entities);
                    }
                    else
                    {
                        Debug.Warn("No type found with name {0} for component on entity {1}.", compName, entity.Name);
                    }
                }
            }
        }

        static void TrySetComponentValue(Component comp, Type type, string propertyName, string value, Dictionary<int, TiledEntity> entities = null)
        {
            var propInfo = type.GetProperty(propertyName);
            if (propInfo != null && propInfo.CanWrite)
            {
                try
                {
                    SetPropertyValue(comp, propInfo, value, entities);
                }
                catch (Exception ex)
                {
                    Debug.Error("Failed to set property {0} on {1}: {2}", propertyName, type.Name, ex.Message);
                }
            }
            else
            {
                var fieldInfo = type.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    try
                    {
                        SetFieldValue(comp, fieldInfo, value, entities);
                    }
                    catch (Exception ex)
                    {
                        Debug.Error("Failed to set field {0} on {1}: {2}", propertyName, type.Name, ex.Message);
                    }
                }
                else
                {
                    Debug.Warn("Property or field {0} not found on {1}", propertyName, type.Name);
                }
            }
        }

        static void SetFieldValue(object obj, FieldInfo field, string value, Dictionary<int, TiledEntity> entities)
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
            else if (field.FieldType == typeof(TiledEntity) && !string.IsNullOrEmpty(value))
                field.SetValue(obj, entities[Convert.ToInt32(value)]);
            else
                throw new NotImplementedException();
        }

        static void SetPropertyValue(object obj, PropertyInfo property, string value, Dictionary<int, TiledEntity> entities)
        {
            if (property.PropertyType == typeof(int))
                property.SetValue(obj, Convert.ToInt32(value));
            else if (property.PropertyType == typeof(float))
                property.SetValue(obj, float.Parse(value));
            else if (property.PropertyType == typeof(string))
                property.SetValue(obj, value);
            else if (property.PropertyType == typeof(bool))
                property.SetValue(obj, bool.Parse(value));
            else if (property.PropertyType.IsEnum)
                property.SetValue(obj, Enum.Parse(property.PropertyType, value));
            else if (property.PropertyType == typeof(TiledEntity) && !string.IsNullOrEmpty(value))
                property.SetValue(obj, entities[Convert.ToInt32(value)]);
            else
                throw new NotImplementedException();
        }

        public static void SetEntityValuesFromProperties(TiledEntity entity, Dictionary<string, string> properties, Dictionary<int, TiledEntity> entities)
        {
            if (properties.TryGetValue("Tag", out var tag))
                entity.Tag = (int)Enum.Parse(typeof(Tag), tag);
        }
    }
}
