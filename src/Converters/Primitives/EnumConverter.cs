using System;
using System.Text;
using Json.Data;

namespace Json.Converters.Primitives
{
    public class EnumConverter : IJsonConverter
    {
        private readonly JsonSettings settings;

        public EnumConverter(JsonSettings settings)
        {
            this.settings = settings;
        }

        public object Read(Type type, string json)
        {
            if (settings.EnumFormat == EnumFormats.Number)
            {
                if (int.TryParse(json, out int number)) return number;
                else throw new Exception();
            }
            else
            {
                try
                {
                    return Enum.Parse(type, json);
                }
                catch (System.Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public void Write(object value, StringBuilder writer)
        {
            if (settings.EnumFormat == EnumFormats.Number)
            {
                writer.Append(((int)value).ToString());
            }
            else
            {
                writer.Append(value.ToString());
            }
        }
    }
}