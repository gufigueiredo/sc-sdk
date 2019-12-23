using System;
using System.Diagnostics.Tracing;

namespace SC.SDK.NetStandard.BuildingBlocks.Logging
{
    public interface ILogger
    {
        void Log(string message, EventLevel level = EventLevel.Verbose);
        void LogError(Exception ex, string message);
        void LogInformation(string message);
        void LogWarning(string message);
    }
}
