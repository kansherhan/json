using System;
using System.Globalization;
using System.Text;

namespace Json.Converters.Primitives
{
    public class DoubleConverter : IJsonConverter
    {
        private readonly CultureInfo cultureInfo;

        public DoubleConverter(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
        }

        public object Read(Type type, string json)
        {
            if (double.TryParse(json, out double number)) return number;
            else throw new Exception();
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append(((double)value).ToString(cultureInfo));
        }
    }
}
