using System;
using System.Globalization;
using System.Text;
using Json.Data;

namespace Json.Converters.Primitives
{
    public class DateTimeConverter : IJsonConverter
    {
        private readonly JsonSettings settings;

        public DateTimeConverter(JsonSettings settings)
        {
            this.settings = settings;
        }

        public object Read(Type type, string json)
        {
            if (DateTime.TryParseExact(json, settings.DateTimeFormat, settings.Culture, DateTimeStyles.None, out DateTime date)) return date;
            else throw new Exception();
        }

        public void Write(object value, StringBuilder writer)
        {
            var date = (DateTime)value;

            writer.Append('"');
            writer.Append(date.ToString(settings.DateTimeFormat));
            writer.Append('"');
        }
    }
}
