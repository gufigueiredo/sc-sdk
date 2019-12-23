namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class RequestHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public RequestHeader(string key, string value)
        {
            Key = key;
            Value = value;
        }
        public static RequestHeader Create(string key, string value)
        {
            return new RequestHeader(key, value);
        }
    }
}
