using System;
using System.Text;

namespace Json.Converters.Primitives
{
    public class DateTimeConverter : IJsonConverter
    {
        public object Read(Type type, string json)
        {
            if (DateTime.TryParse(json, out DateTime date)) return date;
            else throw new Exception();
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append('"');
            writer.Append(((DateTime)value).ToString(JsonConvert.DateTimeFormat));
            writer.Append('"');
        }
    }
}
