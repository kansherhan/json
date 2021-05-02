using Json.Attributes;
using Json.Converters.Collections;
using Json.Data;
using Json.Serialization;
using Json.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Json.Converters
{
    public class ObjectConverter : CollectionConverter
    {
        public const BindingFlags BindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        public ObjectConverter(Dictionary<Type, IJsonConverter> converters) : base(converters) { }

        public override object Read(Type type, string json)
        {
            var elems = json.SplitToElements();

            if (elems.Length % 2 == 0)
            {
                var instance = FormatterServices.GetUninitializedObject(type);

                var fieldDatas = GetPropertyDatas(type.GetFields(BindingFlag));
                var propertyDatas = GetPropertyDatas(type.GetProperties(BindingFlag));

                var reader = new JsonReader(converters);

                for (int i = 0; i < elems.Length; i += 2)
                {
                    if (elems[i].Length > 2)
                    {
                        var key = elems[i].RemoveQuotes();
                        var value = elems[i + 1];

                        if (fieldDatas.TryGetValue(key, out PropertyData fieldData))
                        {
                            var fieldInfo = (FieldInfo)fieldData.MemberInfo;
                            var fieldValue = reader.ParseObject(fieldInfo.FieldType, value);

                            fieldInfo.SetValue(instance, fieldValue);
                        }
                        else if (propertyDatas.TryGetValue(key, out PropertyData propertyData))
                        {
                            var propertyInfo = (PropertyInfo)propertyData.MemberInfo;

                            if (propertyInfo.CanRead)
                            {
                                var propertyValue = reader.ParseObject(propertyInfo.PropertyType, value);

                                propertyInfo.SetValue(instance, propertyValue, null);
                            }
                        }
                    }
                }

                return instance;
            }
            else return null;
        }

        public override void Write(object value, StringBuilder writer)
        {
            writer.Append('{');

            var type = value.GetType();

            var fieldInfos = GetSerializebleMembers(type.GetFields(BindingFlag));
            var propertyInfos = GetSerializebleMembers(type.GetProperties(BindingFlag));

            var fieldDatas = GetPropertyDatas(fieldInfos);
            var propertyDatas = GetPropertyDatas(propertyInfos);

            var jsonWriter = new JsonWriter(converters);

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var data = fieldDatas[fieldInfos[i].Name];

                var fieldKey = data.JsonName;
                var fieldValue = fieldInfos[i].GetValue(value);

                if (i > 0) writer.Append(',');

                writer.Append('"');
                writer.Append(fieldKey);
                writer.Append("\":");

                jsonWriter.Write(fieldValue, writer);
            }

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                var propertyInfo = propertyInfos[i];

                if (propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
                {
                    var propertyKey = propertyDatas[propertyInfo.Name].JsonName;
                    var propertyValue = propertyInfo.GetValue(value, null);

                    if (i > 0) writer.Append(',');

                    writer.Append('"');
                    writer.Append(propertyKey);
                    writer.Append("\":");

                    jsonWriter.Write(propertyValue, writer);
                }
            }

            writer.Append('}');
        }

        public static Member[] GetSerializebleMembers<Member>(Member[] members) where Member : MemberInfo
        {
            var result = new List<Member>();

            foreach (var member in members)
            {
                if (!member.IsDefined(typeof(JsonIgnoreAttribute), true))
                {
                    result.Add(member);
                }
            }

            return result.ToArray();
        }

        public static Dictionary<string, PropertyData> GetPropertyDatas<Member>(Member[] members) where Member : MemberInfo
        {
            var memberDatas = new Dictionary<string, PropertyData>(StringComparer.OrdinalIgnoreCase);

            foreach (var member in members)
            {
                var name = member.Name;

                if (member.IsDefined(typeof(JsonPropertyAttribute), true))
                {
                    var attribute = (JsonPropertyAttribute)member.GetCustomAttributes(typeof(JsonPropertyAttribute), true)[0];

                    memberDatas.Add(member.Name, new PropertyData(name, attribute.Name, member));
                }
                else
                {
                    memberDatas.Add(member.Name, new PropertyData(name, member));
                }
            }

            return memberDatas;
        }
    }
}
