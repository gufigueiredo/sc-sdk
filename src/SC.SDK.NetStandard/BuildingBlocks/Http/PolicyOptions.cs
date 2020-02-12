using System;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public class PolicyOptions
    {
        public RetryPolicyOptions RetryOptions { get; set; }
        public CircuitBreakerPolicyOptions CircuitBreakerOptions { get; set; }

        public PolicyOptions()
        {
            RetryOptions = new RetryPolicyOptions();
            CircuitBreakerOptions = new CircuitBreakerPolicyOptions();
        }
    }

    public class RetryPolicyOptions
    {
        public int Retries { get; set; } = 3;
        public int TimeBetweenRetries { get; set; } = 2;
    }

    public class CircuitBreakerPolicyOptions
    {
        public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(60);
        public double FailureThreshold { get; set; } = 0.5;
        public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(60);
        public int MinimumThroughput { get; set; } = 10;
    }
}
