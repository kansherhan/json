using Json.Serialization;
using Json.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Json.Converters.Collections
{
    public class ArrayConverter : CollectionConverter
    {
        public ArrayConverter(Dictionary<Type, IJsonConverter> converters) : base(converters) { }

        public override object Read(Type type, string json)
        {
            if (json.StartsAndEndsWith('[', ']'))
            {
                var elems = json.SplitToElements();
                var arrayType = type.GetElementType();
                var array = Array.CreateInstance(arrayType, elems.Length);

                var reader = new JsonReader(converters);

                for (int i = 0; i < elems.Length; i++)
                {
                    var value = reader.ParseObject(arrayType, elems[i]);

                    array.SetValue(value, i);
                }

                return array;
            }
            else return null;
        }

        public override void Write(object value, StringBuilder writer)
        {
            writer.Append('[');

            var jsonWriter = new JsonWriter(converters);

            var array = value as Array;

            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0) writer.Append(',');

                var arrayValue = jsonWriter.GetJson(array.GetValue(i));

                writer.Append(arrayValue);
            }

            writer.Append(']');
        }
    }
}
