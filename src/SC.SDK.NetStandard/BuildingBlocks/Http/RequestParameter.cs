namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class RequestParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public RequestParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public static RequestParameter Create(string name, string value)
        {
            return new RequestParameter(name, value);
        }
    }
}
