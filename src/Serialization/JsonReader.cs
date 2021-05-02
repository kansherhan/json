using Json.Converters;
using Json.Converters.Collections;
using Json.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Json.Serialization
{
    public class JsonReader
    {
        private readonly Dictionary<Type, IJsonConverter> converters;

        public JsonReader(Dictionary<Type, IJsonConverter> converters)
        {
            this.converters = converters;
        }

        public T Parse<T>(string json) => (T)ParseObject(typeof(T), json);

        public object ParseObject(Type type, string json)
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

            return ParseValue(type, newJson.ToString());
        }

        private object ParseValue(Type type, string json)
        {
            if (json.Length > 0)
            {
                if (converters.TryGetValue(type, out IJsonConverter converter))
                {
                    return converter.Read(type, json);
                }
                else if (type.IsArray)
                {
                    var arrayConvareter = new ArrayConverter(converters);

                    return arrayConvareter.Read(type, json);
                }
                else if (type.IsGenericType)
                {
                    var typeDefinition = type.GetGenericTypeDefinition();

                    if (typeDefinition == typeof(List<>))
                    {
                        var listconverter = new ListConverter(converters);

                        return listconverter.Read(type, json);
                    }
                    else if (typeDefinition == typeof(Dictionary<,>))
                    {
                        var dictConverter = new DictionaryConverter(converters);

                        return converter.Read(type, json);
                    }
                }
                else
                {
                    var objectConverter = new ObjectConverter(converters);

                    return objectConverter.Read(type, json);
                }
            }

            return null;
        }
    }
}
