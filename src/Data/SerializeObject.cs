using System.Reflection;

namespace Json.Data
{
    public class SerializeObject : JsonObject
    {
        public bool IsProperty { get; set; }

        public string PropertyName { get; set; }

        public MemberInfo MemberInfo { get; set; }

        public SerializeObject(string propertyName, MemberInfo memberInfo) : base(propertyName, null)
        {
            IsProperty = memberInfo.GetType() == typeof(PropertyInfo);

            PropertyName = propertyName;

            MemberInfo = memberInfo;
        }

        public SerializeObject(string propertyName, string jsonName, MemberInfo memberInfo) : this(propertyName, memberInfo)
        {
            Key = jsonName;
        }

        public SerializeObject(string propertyName, string jsonName, string jsonValue, MemberInfo memberInfo) : this(propertyName, jsonName, memberInfo)
        {
            Value = jsonValue;
        }
    }
}