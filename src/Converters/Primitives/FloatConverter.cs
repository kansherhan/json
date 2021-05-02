using System;
using System.Globalization;
using System.Text;

namespace Json.Converters.Primitives
{
    public class FloatConverter : IJsonConverter
    {
        private readonly CultureInfo cultureInfo;

        public FloatConverter(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
        }

        public object Read(Type type, string json)
        {
            if (float.TryParse(json, out float number)) return number;
            else throw new Exception();
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append(((float)value).ToString(cultureInfo));
        }
    }
}
