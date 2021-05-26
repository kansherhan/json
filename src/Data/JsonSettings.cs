using System.Globalization;

namespace Json.Data
{
    public class JsonSettings
    {
        public FloatFormats FloatFormat { get; set; } = FloatFormats.Float;

        public EnumFormats EnumFormat { get; set; } = EnumFormats.Number;

        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";

        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    }
}