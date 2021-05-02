using Json.Serialization;
using Json.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Json.Converters.Collections
{
    public class ListConverter : CollectionConverter
    {
        public ListConverter(Dictionary<Type, IJsonConverter> converters) : base(converters) { }

        public override object Read(Type type, string json)
        {
            if (json.StartsAndEndsWith('[', ']'))
            {
                var jsonReader = new JsonReader(converters);

                var elems = json.SplitToElements();
                var listType = type.GetGenericArguments()[0];

                var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Length });

                for (int i = 0; i < elems.Length; i++)
                {
                    var value = jsonReader.ParseObject(listType, elems[i]);

                    list.Add(value);
                }

                return list;
            }
            else return null;
        }

        public override void Write(object value, StringBuilder writer)
        {
            writer.Append('[');

            var list = value as IList;

            var jsonWriter = new JsonWriter(converters);

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) writer.Append(',');

                var itemValue = jsonWriter.GetJson(list[i]);

                writer.Append(itemValue);
            }

            writer.Append(']');
        }
    }
}
