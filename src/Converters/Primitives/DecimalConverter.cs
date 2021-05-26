using System;
using System.Text;
using Json.Data;

namespace Json.Converters.Primitives
{
    public class DecimalConverter : IJsonConverter
    {
        private readonly JsonSettings settings;

        public DecimalConverter(JsonSettings settings)
        {
            this.settings = settings;
        }

        public object Read(Type type, string json)
        {
            if (decimal.TryParse(json, out decimal number)) return number;
            else
            {
                throw new Exception();
            }
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append(Convert.ToDecimal(value).ToString(settings.Culture));
        }
    }
}