using System.Reflection;

namespace Json.Data
{
    public struct PropertyData
    {
        public string JsonName { get; set; }
        public string PropertyName { get; set; }

        public MemberInfo MemberInfo { get; set; }

        public PropertyData(string propertyName, MemberInfo memberInfo)
        {
            PropertyName = propertyName;
            JsonName = propertyName;

            MemberInfo = memberInfo;
        }

        public PropertyData(string propertyName, string jsonName, MemberInfo memberInfo)
        {
            PropertyName = propertyName;
            JsonName = jsonName;
            MemberInfo = memberInfo;
        }
    }
}
