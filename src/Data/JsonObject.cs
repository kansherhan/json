namespace Json.Data
{
    public class JsonObject
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public JsonObject(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}