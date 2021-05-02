using System.Collections.Generic;
using System.Text;

namespace Json.Utils
{
    public static class Extensions
    {
        public static string RemoveQuotes(this string text) => text.Substring(1, text.Length - 2);

        public static int AppendUntilStringEnd(this int start, string json, StringBuilder builder)
        {
            builder.Append(json[start]);

            for (int i = start + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    builder.Append(json[i]);
                    builder.Append(json[i + 1]);

                    i++;
                }
                else if (json[i] == '"')
                {
                    builder.Append(json[i]);

                    return i;
                }
                else
                {
                    builder.Append(json[i]);
                }
            }

            return json.Length - 1;
        }

        public static string[] SplitToElements(this string json)
        {
            if (json.Length > 2)
            {
                var result = new List<string>();
                var builder = new StringBuilder();

                var parseDepth = 0;

                for (int i = 1; i < json.Length - 1; i++)
                {
                    var symbol = json[i];

                    if (symbol == '[' || symbol == '{')
                    {
                        parseDepth++;

                        builder.Append(symbol);
                    }
                    else if (symbol == ']' || symbol == '}')
                    {
                        parseDepth--;

                        builder.Append(symbol);
                    }
                    else if (symbol == '"')
                    {
                        i = i.AppendUntilStringEnd(json, builder);
                    }
                    else if (symbol == ',' || symbol == ':')
                    {
                        if (parseDepth == 0)
                        {
                            result.Add(builder.ToString());

                            builder.Clear();
                        }
                        else
                        {
                            builder.Append(symbol);
                        }
                    }
                    else
                    {
                        builder.Append(symbol);
                    }
                }

                result.Add(builder.ToString());

                return result.ToArray();
            }
            else return new string[0];
        }

        public static bool StartsAndEndsWith(this string text, char start, char end)
        {
            return text[0] == start && text[text.Length - 1] == end;
        }
    }
}
