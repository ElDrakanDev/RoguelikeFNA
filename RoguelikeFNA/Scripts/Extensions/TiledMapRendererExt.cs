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

        public static void CreateObjects(this TiledMapRenderer map)
        {
            var entities = map.TiledMap.GetObjectGroup("Entities");
            if (entities == null)
                return;

            foreach (var obj in entities.Objects)
            {
                var entity = new TiledEntity(obj.Name, obj.Id);
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
                entity.AddComponent((Component)prefabComponent);
                var fields = prefabComponent.GetType().GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                foreach (var field in fields)
                {
                    if (obj.Properties.TryGetValue(field.Name, out string value))
                    {
                        SetFieldValue(prefabComponent, field, value);
                    }
                }

                prefabComponent.LoadPrefab();

            }
        }

        static void SetFieldValue(object obj, FieldInfo field, string value)
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
            else
                throw new NotImplementedException();
        }

        static void SetEntityFields(TmxObject obj, TiledEntity entity)
        {
            if (obj.Properties.TryGetValue("Tag", out var tag))
                entity.Tag = (int)Enum.Parse(typeof(Tag), tag);
        }
    }
}
