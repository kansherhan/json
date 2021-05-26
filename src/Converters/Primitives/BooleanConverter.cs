using System;
using System.Text;

namespace Json.Converters.Primitives
{
    public class BooleanConverter : IJsonConverter
    {
        public object Read(Type type, string json)
        {
            if (bool.TryParse(json, out bool result))
            {
                return result;
            }
            else
            {
                throw new Exception();
            }
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append((bool)value ? JsonConvert.TrueString : JsonConvert.FalseString);
        }
    }
}