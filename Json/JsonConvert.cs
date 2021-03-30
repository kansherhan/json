using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization;
using Json.Serialization;
using Json.Utils;
using Json.Attributes;

namespace Json
{
    public class JsonConvert
    {
        public static string ToJson(object obj) => new JsonWriter(obj).ToString();

        public static T FromJson<T>(string json) => new JsonReader<T>(json).Value;
    }
}

namespace Json.Serialization
{
    public class JsonWriter
    {
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        public StringBuilder Json { get; }

        public JsonWriter(object obj)
        {
            Json = new StringBuilder();

            AppendValue(obj);
        }

        private void AppendValue(object obj)
        {
            if (obj != null)
            {
                var type = obj.GetType();

                if (type == typeof(string) || type == typeof(char))
                {
                    Json.Append('"');

                    var text = obj.ToString();

                    foreach (var symbol in text)
                    {
                        if (symbol < ' ' || symbol == '"' || symbol == '\\')
                        {
                            Json.Append('\\');

                            int j = "\"\\\n\r\t\b\f".IndexOf(symbol);

                            if (j >= 0)
                            {
                                Json.Append("\"\\nrtbf"[j]);
                            }
                            else
                            {
                                Json.AppendFormat("u{0:X4}", (uint)symbol);
                            }
                        }
                        else
                        {
                            Json.Append(symbol);
                        }
                    }

                    Json.Append('"');
                }
                else if (type == typeof(bool))
                {
                    Json.Append(((bool)obj) ? "true" : "false");
                }
                else if (type.IsPrimitive)
                {
                    if (type == typeof(float))
                    {
                        Json.Append(((float)obj).ToString(CultureInfo.InvariantCulture));
                    }
                    else if (type == typeof(double))
                    {
                        Json.Append(((double)obj).ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Json.Append(obj.ToString());
                    }
                }
                else if (type == typeof(decimal))
                {
                    Json.Append(((decimal)obj).ToString(CultureInfo.InvariantCulture));
                }
                else if (type.IsEnum)
                {
                    var number = (int)obj;

                    Json.Append(number);
                }
                else if (type.IsArray)
                {
                    Json.Append('[');

                    var array = obj as Array;

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i > 0) Json.Append(',');

                        AppendValue(array.GetValue(i));
                    }

                    Json.Append(']');
                }
                else if (type.IsGenericType)
                {
                    var typeDefinition = type.GetGenericTypeDefinition();

                    if (typeDefinition == typeof(List<>))
                    {
                        Json.Append('[');

                        var list = obj as IList;

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (i > 0) Json.Append(',');

                            AppendValue(list[i]);
                        }

                        Json.Append(']');
                    }
                    else if (typeDefinition == typeof(Dictionary<,>))
                    {
                        var keyType = type.GetGenericArguments().First();

                        if (keyType == typeof(string))
                        {
                            Json.Append('{');

                            var dict = obj as IDictionary;

                            var isFirst = true;

                            foreach (DictionaryEntry item in dict)
                            {
                                if (isFirst)
                                {
                                    isFirst = false;
                                }
                                else
                                {
                                    Json.Append(',');
                                }

                                Json.Append('"');
                                Json.Append((string)item.Key);
                                Json.Append("\":");

                                AppendValue(item.Value);
                            }

                            Json.Append('}');
                        }
                        else
                        {
                            Json.Append("\"Key must be a string\"");
                        }
                    }
                }
                else if (type == typeof(DateTime))
                {
                    Json.Append('"');

                    Json.Append(((DateTime)obj).ToString(DateTimeFormat));

                    Json.Append('"');
                }
                else if (type == typeof(Guid))
                {
                    Json.Append('"');

                    Json.Append(obj.ToString());

                    Json.Append('"');
                }
                else AppendObject(obj);
            }
            else Json.Append("null");
        }

        private void AppendObject(object obj)
        {
            Json.Append('{');

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

            var type = obj.GetType();

            var fieldInfos = type.GetFields(flags);
            var propertyInfos = type.GetProperties(flags);

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var field = fieldInfos[i];
                var key = field.GetMemberName();

                if (key.IsNotEmtry())
                {
                    var value = field.GetValue(obj);

                    if (i > 0) Json.Append(',');

                    Json.Append('"');
                    Json.Append(key);
                    Json.Append("\":");

                    AppendValue(value);
                }
            }

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                var property = propertyInfos[i];

                if (property.CanRead && property.GetIndexParameters().Length == 0)
                {
                    var key = property.GetMemberName();

                    if (key.IsNotEmtry())
                    {
                        var value = property.GetValue(obj, null);

                        if (i > 0) Json.Append(',');

                        Json.Append('"');
                        Json.Append(key);
                        Json.Append("\":");

                        AppendValue(value);
                    }
                }
            }

            Json.Append('}');
        }

        public override string ToString() => Json.ToString();
    }

    public class JsonReader<T>
    {
        public T Value { get; }

        public JsonReader(string json)
        {
            var newJson = new StringBuilder();

            for (int i = 0; i < json.Length; i++)
            {
                var symbol = json[i];

                if (symbol == '"')
                {
                    i = i.AppendUntilStringEnd(json, newJson);
                }
                else if (!char.IsWhiteSpace(symbol))
                {
                    newJson.Append(symbol);
                }
            }

            Value = (T)ParseValue(typeof(T), newJson.ToString());
        }

        private object ParseValue(Type type, string json)
        {
            if (json.Length > 0)
            {
                if (type == typeof(string) || type == typeof(char))
                {
                    if (json.Length > 2)
                    {
                        if (type == typeof(char))
                        {
                            return json[1];
                        }
                        else if (type == typeof(string))
                        {
                            var text = new StringBuilder();

                            for (int i = 1; i < json.Length - 1; ++i)
                            {
                                if (json[i] == '\\' && i + 1 < json.Length - 1)
                                {
                                    var j = "\"\\nrtbf/".IndexOf(json[i + 1]);

                                    if (j >= 0)
                                    {
                                        text.Append("\"\\\n\r\t\b\f/"[j]);
                                        ++i;
                                    }
                                    else if (json[i + 1] == 'u' && i + 5 < json.Length - 1)
                                    {
                                        var isUInt = uint.TryParse(json.Substring(i + 2, 4), NumberStyles.AllowHexSpecifier, null, out uint c);

                                        if (isUInt)
                                        {
                                            text.Append((char)c);

                                            i += 5;
                                        }
                                    }
                                }

                                text.Append(json[i]);
                            }

                            return text.ToString();
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else if (type.IsPrimitive)
                {
                    return Convert.ChangeType(json, type, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(decimal))
                {
                    decimal.TryParse(json, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal number);

                    return number;
                }
                else if (type.IsEnum)
                {
                    return ParseValue(typeof(int), json);
                }
                else if (type.IsArray)
                {
                    if (json.StartsAndEndsWith('[', ']'))
                    {
                        var elems = json.SplitToElements();
                        var arrayType = type.GetElementType();
                        var array = Array.CreateInstance(arrayType, elems.Length);

                        for (int i = 0; i < elems.Length; i++)
                        {
                            array.SetValue(ParseValue(arrayType, elems[i]), i);
                        }

                        return array;
                    }
                }
                else if (type == typeof(DateTime))
                {
                    if (DateTime.TryParse(json.RemoveQuotes(), out DateTime date)) return date;
                    else return new DateTime();
                }
                else if (type == typeof(Guid))
                {
                    var isTryParsed = Guid.TryParse(json.RemoveQuotes(), out Guid guid);

                    return isTryParsed ? guid : new Guid();
                }
                else if (type.IsGenericType)
                {
                    if (type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        if (json.StartsAndEndsWith('[', ']'))
                        {
                            var elems = json.SplitToElements();
                            var listType = type.GetGenericArguments().First();

                            var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Length });

                            for (int i = 0; i < elems.Length; i++)
                            {
                                list.Add(ParseValue(listType, elems[i]));
                            }

                            return list;
                        }
                    }
                    else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        Type keyType, valueType;
                        {
                            Type[] args = type.GetGenericArguments();
                            keyType = args[0];
                            valueType = args[1];
                        }

                        if (keyType == typeof(string))
                        {
                            if (json.StartsAndEndsWith('{', '}'))
                            {
                                var elems = json.SplitToElements();

                                if (elems.Length % 2 == 0)
                                {
                                    var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Length / 2 });

                                    for (int i = 0; i < elems.Length; i += 2)
                                    {
                                        if (elems[i].Length > 2)
                                        {
                                            var key = elems[i].RemoveQuotes();
                                            var value = ParseValue(valueType, elems[i + 1]);

                                            dictionary[key] = value;
                                        }
                                    }

                                    return dictionary;
                                }
                            }
                        }
                    }
                }
                else if (json.StartsAndEndsWith('{', '}'))
                {
                    return ParseObject(type, json.SplitToElements());
                }
            }

            return null;
        }

        private object ParseObject(Type type, string[] elems)
        {
            if (elems.Length % 2 == 0)
            {
                var instance = FormatterServices.GetUninitializedObject(type);

                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

                var nameToField = type.GetFields(flags).GetMembersName();
                var nameToProperty = type.GetProperties(flags).GetMembersName();

                for (int i = 0; i < elems.Length; i += 2)
                {
                    if (elems[i].Length > 2)
                    {
                        var key = elems[i].RemoveQuotes();
                        var value = elems[i + 1];

                        if (nameToField.TryGetValue(key, out FieldInfo fieldInfo))
                        {
                            fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                        }
                        else if (nameToProperty.TryGetValue(key, out PropertyInfo propertyInfo) && propertyInfo.CanWrite)
                        {
                            propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
                        }
                    }
                }

                return instance;
            }
            else return null;
        }
    }
}

namespace Json.Utils
{
    public static class Extensions
    {
        public static string RemoveQuotes(this string text) => text.Substring(1, text.Length - 2);

        public static bool IsNotEmtry(this string text) => !string.IsNullOrWhiteSpace(text);

        public static int AppendUntilStringEnd(this int start, string json, StringBuilder builder)
        {
            builder.Append(json[start]);

            for (int i = start + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    builder.Append(json[i]);
                    builder.Append(json[i + 1]);

                    i++;
                }
                else if (json[i] == '"')
                {
                    builder.Append(json[i]);

                    return i;
                }
                else
                {
                    builder.Append(json[i]);
                }
            }

            return json.Length - 1;
        }

        public static string[] SplitToElements(this string text)
        {
            if (text.Length > 2)
            {
                var result = new List<string>();
                var builder = new StringBuilder();

                var parseDepth = 0;

                for (int i = 1; i < text.Length - 1; i++)
                {
                    var symbol = text[i];

                    if (symbol == '[' || symbol == '{')
                    {
                        parseDepth++;

                        builder.Append(symbol);
                    }
                    else if (symbol == ']' || symbol == '}')
                    {
                        parseDepth--;

                        builder.Append(symbol);
                    }
                    else if (symbol == '"')
                    {
                        i = i.AppendUntilStringEnd(text, builder);
                    }
                    else if (symbol == ',' || symbol == ':')
                    {
                        if (parseDepth == 0)
                        {
                            result.Add(builder.ToString());

                            builder.Clear();
                        }
                        else
                        {
                            builder.Append(symbol);
                        }
                    }
                    else
                    {
                        builder.Append(symbol);
                    }
                }

                result.Add(builder.ToString());

                return result.ToArray();
            }
            else return new string[0];
        }

        public static bool StartsAndEndsWith(this string text, char start, char end) => text.First() == start && text.Last() == end;

        public static string GetMemberName<Member>(this Member member) where Member : MemberInfo
        {
            if (!member.IsDefined(typeof(JsonIgnoreAttribute), true)) return member.Name;
            else return null;
        }

        public static Dictionary<string, Member> GetMembersName<Member>(this Member[] members) where Member : MemberInfo
        {
            var nameToMember = new Dictionary<string, Member>(StringComparer.OrdinalIgnoreCase);

            foreach (var member in members)
            {
                var name = member.GetMemberName();

                if (name.IsNotEmtry()) nameToMember.Add(member.Name, member);
            }

            return nameToMember;
        }
    }
}

namespace Json.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonMemberAttribute : Attribute
    {
        public string Name { get; set; }

        public JsonMemberAttribute(string name)
        {
            Name = name;
        }
    }
}
