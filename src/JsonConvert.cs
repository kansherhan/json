using Json.Converters;
using Json.Converters.Primitives;
using Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Json
{
    public class JsonConvert
    {
        public const string NullString = "null";
        public const string TrueString = "true";
        public const string FalseString = "false";

        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        public static string ToJson(object value) => ToJson(value, CultureInfo.CurrentCulture);

        public static T FromJson<T>(string json) => FromJson<T>(json, CultureInfo.CurrentCulture);

        public static string ToJson(object obj, CultureInfo cultureInfo)
        {
            var converters = PrimitiveConverters(cultureInfo);
            var writer = new JsonWriter(converters);

            return writer.GetJson(obj);
        }

        public static T FromJson<T>(string json, CultureInfo cultureInfo)
        {
            var converters = PrimitiveConverters(cultureInfo);
            var reader = new JsonReader(converters);

            return reader.Parse<T>(json);
        }

        public static Dictionary<Type, IJsonConverter> PrimitiveConverters(CultureInfo cultureInfo)
        {
            return new Dictionary<Type, IJsonConverter>()
            {
                { typeof(int), new IntConverter() },
                { typeof(bool), new BooleanConverter() },
                { typeof(float), new FloatConverter(cultureInfo) },
                { typeof(double), new DoubleConverter(cultureInfo) },
                { typeof(string), new StringConverter() },
                { typeof(DateTime), new DateTimeConverter() },
                { typeof(Enum), new EnumConverter() },
                { typeof(decimal), new DecimalConverter(cultureInfo) }
            };
        }
    }
}
