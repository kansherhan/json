using System;
using System.Globalization;
using System.Text;

namespace Json.Converters.Primitives
{
    public class StringConverter : IJsonConverter
    {
        public object Read(Type type, string json)
        {
            if (json.Length > 2)
            {
                var text = new StringBuilder();

                for (int i = 1; i < json.Length - 1; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length - 1)
                    {
                        var j = "\"\\nrtbf/".IndexOf(json[i + 1]);

                        if (j >= 0)
                        {
                            text.Append("\"\\\n\r\t\b\f/"[j]);
                            ++i;
                        }
                        else if (json[i + 1] == 'u' && i + 5 < json.Length - 1)
                        {
                            var isUInt = uint.TryParse(json.Substring(i + 2, 4), NumberStyles.AllowHexSpecifier, null, out uint c);

                            if (isUInt)
                            {
                                text.Append((char)c);

                                i += 5;
                            }
                        }
                    }

                    text.Append(json[i]);
                }

                return text.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append('"');

            var text = value.ToString();

            foreach (var symbol in text)
            {
                if (symbol < ' ' || symbol == '"' || symbol == '\\')
                {
                    writer.Append('\\');

                    int j = "\"\\\n\r\t\b\f".IndexOf(symbol);

                    if (j >= 0)
                    {
                        writer.Append("\"\\nrtbf"[j]);
                    }
                    else
                    {
                        writer.AppendFormat("u{0:X4}", (uint)symbol);
                    }
                }
                else
                {
                    writer.Append(symbol);
                }
            }

            writer.Append('"');
        }
    }
}
