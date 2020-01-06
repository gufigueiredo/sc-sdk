using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SC.SDK.NetStandard.BuildingBlocks.Logging.Sinks
{
    public class LogentriesLogger : ILogger
    {
        public void Log(string message, EventLevel level = EventLevel.Verbose)
        {
            throw new NotImplementedException();
        }

        public void LogError(Exception ex, string message)
        {
            throw new NotImplementedException();
        }

        public void LogInformation(string message)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string message)
        {
            throw new NotImplementedException();
        }
    }
}
