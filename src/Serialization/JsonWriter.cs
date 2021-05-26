using Json.Converters;
using Json.Converters.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Json.Serialization
{
    public class JsonWriter
    {
        private readonly Dictionary<Type, IJsonConverter> converters;

        public JsonWriter(Dictionary<Type, IJsonConverter> converters)
        {
            this.converters = converters;
        }

        public string GetJson(object value)
        {
            var writer = new StringBuilder();

            Write(value, writer);

            return writer.ToString();
        }

        public void Write(object value, StringBuilder writer)
        {
            if (value != null)
            {
                var type = value.GetType();

                if (converters.TryGetValue(type, out IJsonConverter converter))
                {
                    converter.Write(value, writer);
                }
                else if (type.IsArray)
                {
                    var arrayConverter = new ArrayConverter(converters);

                    arrayConverter.Write(value, writer);
                }
                else if (type.IsGenericType)
                {
                    var typeDefinition = type.GetGenericTypeDefinition();

                    if (typeDefinition == typeof(List<>))
                    {
                        var listconverter = new ListConverter(converters);

                        listconverter.Write(value, writer);
                    }
                    else if (typeDefinition == typeof(Dictionary<,>))
                    {
                        var dictConverter = new DictionaryConverter(converters);

                        converter.Write(value, writer);
                    }
                    else
                    {
                        writer.Append(JsonConvert.NullString);
                    }
                }
                else
                {
                    var objectConverter = new ObjectConverter(converters);

                    objectConverter.Write(value, writer);
                }
            }
            else
            {
                writer.Append(JsonConvert.NullString);
            }
        }
    }
}