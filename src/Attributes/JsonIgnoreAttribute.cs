using System;

namespace Json.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonIgnoreAttribute : Attribute { }
}
