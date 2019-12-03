namespace SC.SDK.NetStandard.Crosscutting.Logging
{
    public interface ILogger
    {
         void Log(string message);
         void Log(string message, object source);
    }
}