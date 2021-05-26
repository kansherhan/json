using System;
using System.Collections.Generic;
using Json.Converters;
using Json.Converters.Primitives;
using Json.Data;
using Json.Serialization;

namespace Json
{
    public class JsonConvert
    {
        public const string NullString = "null";
        public const string TrueString = "true";
        public const string FalseString = "false";

        public static string ToJson(object value) => ToJson(value, new JsonSettings());

        public static T FromJson<T>(string json) => FromJson<T>(json, new JsonSettings());

        public static string ToJson(object obj, JsonSettings settings)
        {
            var converters = GetPrimitiveConverters(settings);
            var writer = new JsonWriter(converters);

            return writer.GetJson(obj);
        }

        public static T FromJson<T>(string json, JsonSettings settings)
        {
            var converters = GetPrimitiveConverters(settings);
            var reader = new JsonReader(converters);

            return reader.Parse<T>(json);
        }

        public static Dictionary<Type, IJsonConverter> GetPrimitiveConverters(JsonSettings settings)
        {
            return new Dictionary<Type, IJsonConverter>()
            {
                { typeof(int), new IntConverter() },
                { typeof(bool), new BooleanConverter() },
                { typeof(float), new FloatConverter(settings) },
                { typeof(double), new DoubleConverter(settings) },
                { typeof(string), new StringConverter() },
                { typeof(DateTime), new DateTimeConverter(settings) },
                { typeof(Enum), new EnumConverter(settings) },
                { typeof(decimal), new DecimalConverter(settings) }
            };
        }
    }
}