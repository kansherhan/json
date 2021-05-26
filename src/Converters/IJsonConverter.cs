using System;
using System.Text;

namespace Json.Converters
{
    public interface IJsonConverter
    {
        void Write(object value, StringBuilder writer);
        
        object Read(Type type, string json);
    }
}