using Json.Attributes;
using Json.Data;
using Json.Serialization;
using Json.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Json.Converters
{
    public class ObjectConverter : IJsonConverter
    {
        public const BindingFlags BindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        
        private readonly JsonReader jsonReader;
        private readonly JsonWriter jsonWriter;

        private readonly Dictionary<Type, IJsonConverter> converters;

        public ObjectConverter(Dictionary<Type, IJsonConverter> converters)
        {
            this.converters = converters;

            jsonReader = new JsonReader(converters);
            jsonWriter = new JsonWriter(converters);
        }

        public object Read(Type type, string json)
        {
            var instance = FormatterServices.GetUninitializedObject(type);

            var deserializeObjects = GetDeserializeObjects(type, json);

            foreach (var obj in deserializeObjects)
            {
                var member = obj.MemberInfo;

                if (member is FieldInfo fieldInfo)
                {
                    var key = obj.Key;
                    var value = jsonReader.ParseObject(fieldInfo.FieldType, json);

                    fieldInfo.SetValue(instance, value);
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    var key = obj.Key;
                    var value = jsonReader.ParseObject(propertyInfo.PropertyType, json);

                    propertyInfo.SetValue(instance, value, null);
                }
            }

            return instance;
        }

        public void Write(object obj, StringBuilder writer)
        {
            void AddNewValue(string key, object value, bool addSelector, StringBuilder _writer)
            {
                if (addSelector)
                {
                    writer.Append(",");
                }

                _writer.Append('"');
                _writer.Append(key);
                _writer.Append("\":");

                jsonWriter.Write(value, writer);
            }

            writer.Append('{');

            var objects = GetSerializeObjects(obj.GetType());

            for (int i = 0; i < objects.Length; i++)
            {
                var member = objects[i].MemberInfo;

                if (member is FieldInfo fieldInfo)
                {
                    var key = objects[i].Key;
                    var value = fieldInfo.GetValue(obj);
                    var addSelector = i > 0;

                    AddNewValue(key, value, addSelector, writer);
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    var key = objects[i].Key;
                    var value = propertyInfo.GetValue(obj, null);
                    var addSelector = i > 0;

                    AddNewValue(key, value, addSelector, writer);
                }
            }

            writer.Append('}');
        }

        public static SerializeObject[] GetPropertyDatas<Member>(Member[] members) where Member : MemberInfo
        {
            var propertyDatas = new List<SerializeObject>();

            foreach (var member in members)
            {
                if (!member.IsDefined(typeof(JsonIgnoreAttribute), true))
                {
                    var name = member.Name;

                    if (member.IsDefined(typeof(JsonPropertyAttribute), true))
                    {
                        var attributes = member.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                        var propertyAttribute = (JsonPropertyAttribute)attributes.Where((s) => typeof(JsonPropertyAttribute) == s.GetType()).First();

                        propertyDatas.Add(new SerializeObject(name, propertyAttribute.Name, member));
                    }
                    else
                    {
                        propertyDatas.Add(new SerializeObject(name, member));
                    }
                }
            }

            return propertyDatas.ToArray();
        }

        public static SerializeObject[] GetSerializeObjects(Type type)
        {
            var objects = new List<SerializeObject>();

            var fieldDatas = GetPropertyDatas(type.GetFields(BindingFlag));
            var propertyDatas = GetPropertyDatas(type.GetFields(BindingFlag));

            objects.AddRange(fieldDatas);

            for (int i = 0; i < propertyDatas.Length; i++)
            {
                var property = (PropertyInfo)propertyDatas[i].MemberInfo;

                if (property.CanRead && property.GetIndexParameters().Length == 0)
                {
                    objects.Add(propertyDatas[i]);
                }
            }

            return objects.ToArray();
        }

        public static IEnumerable<SerializeObject> GetDeserializeObjects(Type type, string json)
        {
            var jsonDatas = GetJsonData(json.SplitToElements());
            
            var fieldDatas = GetPropertyDatas(type.GetFields(BindingFlag));
            var propertyDatas = GetPropertyDatas(type.GetProperties(BindingFlag));

            for (int i = 0; i < jsonDatas.Length; i++)
            {
                for (int j = 0; j < fieldDatas.Length; j++)
                {
                    if (fieldDatas[j].Key == jsonDatas[i].Key)
                    {
                        fieldDatas[j].Value = jsonDatas[i].Value;

                        yield return fieldDatas[j];
                    }
                }

                for (int j = 0; j < propertyDatas.Length; j++)
                {
                    if (propertyDatas[j].Key == jsonDatas[i].Key)
                    {
                        var property = (PropertyInfo)propertyDatas[i].MemberInfo;

                        if (property.CanRead && property.GetIndexParameters().Length == 0)
                        {
                            propertyDatas[j].Value = jsonDatas[i].Value;

                            yield return propertyDatas[j];
                        }
                    }
                }
            }
        }

        public static JsonObject[] GetJsonData(string[] elems)
        {
            if (elems.Length % 2 == 0)
            {
                var jsonDatas = new List<JsonObject>();

                for (int i = 0; i < elems.Length; i++)
                {
                    var key = elems[i].RemoveQuotes();
                    var value = elems[i + 1];

                    jsonDatas.Add(new JsonObject(key, value));
                }

                return jsonDatas.ToArray();
            }
            else
            {
                throw new Exception("");
            }
        }
    }
}