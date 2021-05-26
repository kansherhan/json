using System;
using System.Text;
using Json.Data;

namespace Json.Converters.Primitives
{
    public class DoubleConverter : IJsonConverter
    {
        private readonly JsonSettings settings;

        public DoubleConverter(JsonSettings settings)
        {
            this.settings = settings;
        }

        public object Read(Type type, string json)
        {
            if (double.TryParse(json, out double number)) return number;
            else
            {
                throw new Exception();
            }
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append(Convert.ToDouble(value).ToString(settings.Culture));
        }
    }
}