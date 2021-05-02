using Json.Serialization;
using Json.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Json.Converters.Collections
{
    public class DictionaryConverter : CollectionConverter
    {
        public DictionaryConverter(Dictionary<Type, IJsonConverter> converters) : base(converters)
        {
        }

        public override object Read(Type type, string json)
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
                        var reader = new JsonReader(converters);

                        for (int i = 0; i < elems.Length; i += 2)
                        {
                            if (elems[i].Length > 2)
                            {
                                var key = elems[i].RemoveQuotes();
                                var value = reader.ParseObject(valueType, elems[i + 1]);

                                dictionary[key] = value;
                            }
                        }

                        return dictionary;
                    }
                }
            }

            return null;
        }

        public override void Write(object value, StringBuilder writer)
        {
            var type = value.GetType();
            var keyType = type.GetGenericArguments().First();

            if (keyType == typeof(string))
            {
                writer.Append('{');

                var dict = value as IDictionary;

                var jsonWriter = new JsonWriter(converters);

                var isFirst = true;

                foreach (DictionaryEntry entry in dict)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        writer.Append(',');
                    }

                    writer.Append('"');
                    writer.Append((string)entry.Key);
                    writer.Append("\":");

                    var dictValue = jsonWriter.GetJson(entry.Value);

                    writer.Append(dictValue);
                }

                writer.Append('}');
            }
            else
            {
                writer.Append("\"Key must be a string\"");
            }
        }
    }
}
