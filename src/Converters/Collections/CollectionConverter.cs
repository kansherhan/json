using System;
using System.Collections.Generic;
using System.Text;

namespace Json.Converters.Collections
{
    public abstract class CollectionConverter : IJsonConverter
    {
        protected readonly Dictionary<Type, IJsonConverter> converters;

        protected CollectionConverter(Dictionary<Type, IJsonConverter> converters)
        {
            this.converters = converters;
        }

        public abstract object Read(Type type, string json);

        public abstract void Write(object value, StringBuilder writer);
    }
}
