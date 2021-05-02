using System;
using System.Globalization;
using System.Text;

namespace Json.Converters.Primitives
{
    public class DecimalConverter : IJsonConverter
    {
        private readonly CultureInfo cultureInfo;

        public DecimalConverter(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
        }

        public object Read(Type type, string json)
        {
            if (decimal.TryParse(json, out decimal number)) return number;
            else throw new Exception();
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append(((decimal)value).ToString(cultureInfo));
        }
    }
}
