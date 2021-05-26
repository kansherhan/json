using System;
using System.Text;
using Json.Data;

namespace Json.Converters.Primitives
{
    public class FloatConverter : IJsonConverter
    {
        private readonly JsonSettings settings;

        public FloatConverter(JsonSettings settings)
        {
            this.settings = settings;
        }

        public object Read(Type type, string json)
        {
            if (float.TryParse(json, out float number)) return number;
            else
            {
                throw new Exception();
            }
        }

        public void Write(object value, StringBuilder writer)
        {
            if (settings.FloatFormat == FloatFormats.Decimal)
            {
                writer.Append(Convert.ToDecimal(value).ToString(settings.Culture));
            }
            else if (settings.FloatFormat == FloatFormats.Double)
            {
                writer.Append(Convert.ToDouble(value).ToString(settings.Culture));
            }
            else
            {
                writer.Append(((float)value).ToString(settings.Culture));
            }
        }
    }
}
