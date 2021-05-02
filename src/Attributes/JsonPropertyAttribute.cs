using System;

namespace Json.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public JsonPropertyAttribute(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }
            else
            {
                throw new Exception("Property name is emtry or null.");
            }
        }
    }
}
