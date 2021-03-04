using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Json
{
    public class JsonConvert
    {
        public static string ToJson(object obj)
        {
            var writer = new JsonWriter(obj);

            return writer.GetJson();
        }

        public static T FromJson<T>(string json)
        {
            var reader = new JsonReader<T>(json);

            return reader.GetValue();
        }

        public class JsonWriter
        {
            public StringBuilder Json { get; private set; }

            public JsonWriter(object obj)
            {
                Json = new StringBuilder();

                AppendValue(obj);
            }

            public string GetJson()
            {
                return Json.ToString();
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
                    else if (type == typeof(byte) || type == typeof(sbyte))
                    {
                        Json.Append(obj.ToString());
                    }
                    else if (type == typeof(short) || type == typeof(ushort))
                    {
                        Json.Append(obj.ToString());
                    }
                    else if (type == typeof(int) || type == typeof(uint))
                    {
                        Json.Append(obj.ToString());
                    }
                    else if (type == typeof(long) || type == typeof(ulong))
                    {
                        Json.Append(obj.ToString());
                    }
                    else if (type == typeof(float))
                    {
                        var number = (float)obj;
                        var text = number.ToString(CultureInfo.InvariantCulture);

                        Json.Append(text);
                    }
                    else if (type == typeof(double))
                    {
                        var number = (double)obj;
                        var text = number.ToString(CultureInfo.InvariantCulture);

                        Json.Append(text);
                    }
                    else if (type == typeof(decimal))
                    {
                        var number = (decimal)obj;
                        var text = number.ToString(CultureInfo.InvariantCulture);

                        Json.Append(text);
                    }
                    else if (type == typeof(bool))
                    {
                        var value = (bool)obj;
                        var text = value ? "true" : "false";

                        Json.Append(text);
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
                            if (i > 0)
                            {
                                Json.Append(',');
                            }

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
                                if (i > 0)
                                {
                                    Json.Append(',');
                                }

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

                        var datetime = (DateTime)obj;
                        var text = datetime.ToString("yyyy-MM-ddTHH:mm:ss");

                        Json.Append(text);

                        Json.Append('"');
                    }
                    else
                    {
                        Json.Append('{');

                        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        for (int i = 0; i < fieldInfos.Length; i++)
                        {
                            if (fieldInfos[i].IsDefined(typeof(IgnoreDataMemberAttribute), true) == false)
                            {
                                object value = fieldInfos[i].GetValue(obj);

                                if (value != null)
                                {
                                    if (i > 0)
                                    {
                                        Json.Append(',');
                                    }

                                    Json.Append('"');
                                    Json.Append(GetMemberName(fieldInfos[i]));
                                    Json.Append("\":");

                                    AppendValue(value);
                                }
                            }
                        }

                        PropertyInfo[] propertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        for (int i = 0; i < propertyInfo.Length; i++)
                        {
                            if (propertyInfo[i].CanRead || propertyInfo[i].IsDefined(typeof(IgnoreDataMemberAttribute), true) == false)
                            {
                                object value = propertyInfo[i].GetValue(obj, null);

                                if (value != null)
                                {
                                    if (i > 0)
                                    {
                                        Json.Append(',');
                                    }

                                    Json.Append('"');
                                    Json.Append(GetMemberName(propertyInfo[i]));
                                    Json.Append("\":");

                                    AppendValue(value);
                                }
                            }
                        }

                        Json.Append('}');
                    }
                }
                else
                {
                    Json.Append("null");
                }
            }

            private string GetMemberName(MemberInfo member)
            {
                if (member.IsDefined(typeof(DataMemberAttribute), true))
                {
                    var dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);

                    if (string.IsNullOrEmpty(dataMemberAttribute.Name) == false)
                    {
                        return dataMemberAttribute.Name;
                    }
                }

                return member.Name;
            }
        }

        public class JsonReader<T>
        {
            public T Value { get; private set; }

            public const string DateTimePattern = @"^(?<Year>\d{4})-(?<Month>\d{2})-(?<Day>\d{2})T(?<Hour>\d{2}):(?<Minute>\d{2}):(?<Second>\d{2})$";

            public JsonReader(string json)
            {
                var newJson = new StringBuilder();

                for (int i = 0; i < json.Length; i++)
                {
                    var symbol = json[i];

                    if (symbol != '"' || char.IsWhiteSpace(symbol))
                    {
                        newJson.Append(symbol);
                    }
                    else
                    {
                        i = AppendUntilStringEnd(true, i, json, newJson);
                    }
                }

                Value = (T)ParseValue(typeof(T), newJson.ToString());
            }

            public T GetValue()
            {
                return Value;
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
                        if (json.First() == '[' && json.Last() == ']')
                        {
                            var elems = Split(json);
                            var arrayType = type.GetElementType();
                            var newArray = Array.CreateInstance(arrayType, elems.Count);

                            for (int i = 0; i < elems.Count; i++)
                            {
                                newArray.SetValue(ParseValue(arrayType, elems[i]), i);
                            }

                            return newArray;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (type == typeof(DateTime))
                    {
                        var regex = new Regex(DateTimePattern);
                        var input = json.Substring(1, json.Length - 2);

                        if (regex.IsMatch(input))
                        {
                            var elems = new List<string>() { "\"Year\"", "\"Month\"", "\"Day\"", "\"Hour\"", "\"Minute\"", "\"Second\"" };

                            var match = regex.Match(input);
                            var groups = match.Groups;

                            for (int i = 0; i < groups.Count - 1; i++)
                            {
                                var index = i + i;
                                var key = elems[index].Substring(1, elems[index].Length - 2);
                                var value = groups[key].Value;

                                elems.Insert(index + 1, value);
                            }

                            var date = (Date)ParseObject(typeof(Date), elems.ToArray());

                            return date.GetDateTime();
                        }
                        else
                        {
                            return new DateTime();
                        }
                    }
                    else if (type.IsGenericType)
                    {
                        if (type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            if (json.First() == '[' && json.Last() == ']')
                            {
                                var elems = Split(json);
                                var listType = type.GetGenericArguments().First();
                                var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });

                                for (int i = 0; i < elems.Count; i++)
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
                                if (json.First() == '{' && json.Last() == '}')
                                {
                                    var elems = Split(json);

                                    if (elems.Count % 2 == 0)
                                    {
                                        var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });

                                        for (int i = 0; i < elems.Count; i += 2)
                                        {
                                            if (elems[i].Length > 2)
                                            {
                                                var keyValue = elems[i].Substring(1, elems[i].Length - 2);

                                                var val = ParseValue(valueType, elems[i + 1]);

                                                dictionary[keyValue] = val;
                                            }
                                        }

                                        return dictionary;
                                    }
                                }
                            }
                        }

                        return null;
                    }
                    else if (json.First() == '{' && json.Last() == '}')
                    {
                        var elems = Split(json);

                        if (elems.Count % 2 == 0)
                        {
                            return ParseObject(type, elems.ToArray());
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (json == "null")
                    {
                        return null;
                    }
                }

                return null;
            }

            private object ParseObject(Type type, string[] elems)
            {
                var instance = FormatterServices.GetUninitializedObject(type);

                if (elems.Length % 2 == 0)
                {
                    var nameToField = GetMembersName(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                    var nameToProperty = GetMembersName(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));

                    for (int i = 0; i < elems.Length; i += 2)
                    {
                        if (elems[i].Length > 2)
                        {
                            var key = elems[i].Substring(1, elems[i].Length - 2);
                            var value = elems[i + 1];

                            if (nameToField.TryGetValue(key, out FieldInfo fieldInfo))
                            {
                                fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                            }
                            else if (nameToProperty.TryGetValue(key, out PropertyInfo propertyInfo))
                            {
                                propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
                            }
                        }
                    }
                }

                return instance;
            }

            private Dictionary<string, Member> GetMembersName<Member>(Member[] members) where Member : MemberInfo
            {
                var nameToMember = new Dictionary<string, Member>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < members.Length; i++)
                {
                    var member = members[i];

                    if (!member.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    {
                        var name = member.Name;

                        if (member.IsDefined(typeof(DataMemberAttribute), true))
                        {
                            var dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);

                            if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                            {
                                name = dataMemberAttribute.Name;
                            }
                        }

                        nameToMember.Add(name, member);
                    }
                }

                return nameToMember;
            }

            private List<string> Split(string json)
            {
                var result = new List<string>();
                var stringBuilder = new StringBuilder();

                if (json.Length > 2)
                {
                    var parseDepth = 0;

                    for (int i = 1; i < json.Length - 1; i++)
                    {
                        var symbol = json[i];

                        if (symbol == '[' || symbol == '{')
                        {
                            parseDepth++;

                            stringBuilder.Append(symbol);
                        }
                        else if (symbol == ']' || symbol == '}')
                        {
                            parseDepth--;

                            stringBuilder.Append(symbol);
                        }
                        else if (symbol == '"')
                        {
                            i = AppendUntilStringEnd(true, i, json, stringBuilder);
                        }
                        else if (symbol == ',' || symbol == ':')
                        {
                            if (parseDepth == 0)
                            {
                                result.Add(stringBuilder.ToString());

                                stringBuilder.Clear();
                            }
                            else
                            {
                                stringBuilder.Append(symbol);
                            }
                        }
                        else
                        {
                            stringBuilder.Append(symbol);
                        }
                    }

                    result.Add(stringBuilder.ToString());
                }

                return result;
            }

            private int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json, StringBuilder stringBuilder)
            {
                stringBuilder.Append(json[startIdx]);

                for (int i = startIdx + 1; i < json.Length; i++)
                {
                    if (json[i] == '\\')
                    {
                        if (appendEscapeCharacter)
                        {
                            stringBuilder.Append(json[i]);
                        }

                        stringBuilder.Append(json[i + 1]);

                        i++;
                    }
                    else if (json[i] == '"')
                    {
                        stringBuilder.Append(json[i]);

                        return i;
                    }
                    else
                    {
                        stringBuilder.Append(json[i]);
                    }
                }

                return json.Length - 1;
            }

            public class Date
            {
                public int Year { get; set; }

                public int Month { get; set; }

                public int Day { get; set; }

                public int Hour { get; set; }

                public int Minute { get; set; }

                public int Second { get; set; }

                public DateTime GetDateTime()
                {
                    return new DateTime(Year, Month, Day, Hour, Minute, Second);
                }
            }
        }
    }
}