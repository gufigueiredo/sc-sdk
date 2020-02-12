using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SC.SDK.NetStandard.BuildingBlocks.Http
{
    public static class HttpPolicyBuilder
    {
        public static AsyncPolicyWrap<IRestResponse<T>> BuildPolicy<T>(PolicyOptions options = null)
        {
            if (options == null)
                options = new PolicyOptions();

            var retryPolicy = Policy.
                HandleResult<IRestResponse<T>>(r => r.StatusCode == HttpStatusCode.GatewayTimeout ||
                                                    r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                    r.StatusCode == HttpStatusCode.RequestTimeout ||
                                                    r.StatusCode == HttpStatusCode.BadGateway ||
                                                    r.StatusCode == (HttpStatusCode)429 ||
                                                    r.ResponseStatus == ResponseStatus.TimedOut ||
                                                    r.ResponseStatus == ResponseStatus.Error)
                .WaitAndRetryAsync(options.RetryOptions.Retries, retryAttemp => TimeSpan.FromSeconds(Math.Pow(options.RetryOptions.TimeBetweenRetries, retryAttemp)), onRetry: (response, delay, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    if (!string.IsNullOrWhiteSpace(response.Result?.ErrorMessage))
                        logger.LogError(response.Exception, $"[ExecuteRequest][Retry][{response.Result?.ErrorMessage}][Attempt {retryCount} of {options.RetryOptions.Retries}]");
                    else
                        logger.LogError(response.Exception, $"[ExecuteRequest][Retry][Failed with StatusCode: {response.Result?.StatusCode.ToString()}][Attempt {retryCount} of {options.RetryOptions.Retries}]");
                })
                .WithPolicyKey("RetryPolicy");

            //var circuitBreakerPolicy = Policy
            //       .HandleResult<IRestResponse<T>>(r => r.StatusCode == HttpStatusCode.GatewayTimeout ||
            //                                       r.StatusCode == HttpStatusCode.ServiceUnavailable ||
            //                                       r.StatusCode == HttpStatusCode.RequestTimeout ||
            //                                       r.StatusCode == HttpStatusCode.BadGateway ||
            //                                       r.StatusCode == (HttpStatusCode)429 ||
            //                                       r.ResponseStatus == ResponseStatus.TimedOut ||
            //                                       r.ResponseStatus == ResponseStatus.Error)
            //       .CircuitBreakerAsync(
            //           handledEventsAllowedBeforeBreaking: options.ExceptionsAllowedBeforBreak,
            //           durationOfBreak: options.BreakDuration,
            //           onBreak: (response, delay, context) =>
            //           {
            //               var logger = context.GetLogger();
            //               logger.LogError($"[ExecuteRequest][CircuitBreaker] Breaking the circuit for {delay.TotalMilliseconds}ms due to {response.Result?.ErrorMessage}");
            //           },
            //           onReset: (context) =>
            //           {
            //               var logger = context.GetLogger();
            //               logger.LogInformation($"[ExecuteRequest][CircuitBreaker] Circuit is closed now");
            //           },
            //           onHalfOpen: () =>
            //           {
            //               Trace.WriteLine($"[ExecuteRequest][CircuitBreaker] Circuit is half-open. Trying the next call...");
            //           }
            //       ).WithPolicyKey("CircuitBreakerPolicy");

            var circuitBreakerPolicy = Policy
                   .HandleResult<IRestResponse<T>>(r => r.StatusCode == HttpStatusCode.GatewayTimeout ||
                                                   r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                   r.StatusCode == HttpStatusCode.RequestTimeout ||
                                                   r.StatusCode == HttpStatusCode.BadGateway ||
                                                   r.StatusCode == (HttpStatusCode)429 ||
                                                   r.ResponseStatus == ResponseStatus.TimedOut ||
                                                   r.ResponseStatus == ResponseStatus.Error)
                   .AdvancedCircuitBreakerAsync(
                        failureThreshold: 0.5,
                        samplingDuration: TimeSpan.FromSeconds(60),
                        minimumThroughput: 10,
                        durationOfBreak: TimeSpan.FromSeconds(60),
                        onBreak: (response, delay, context) =>
                        {
                            var logger = context.GetLogger();
                            logger.LogError($"[ExecuteRequest][CircuitBreaker] Breaking the circuit for {delay.TotalMilliseconds}ms due to {response.Result?.ErrorMessage}");
                        },
                        onReset: (context) =>
                        {
                            var logger = context.GetLogger();
                            logger.LogInformation($"[ExecuteRequest][CircuitBreaker] Circuit is closed now");
                        },
                        onHalfOpen: () =>
                        {
                            Trace.WriteLine($"[ExecuteRequest][CircuitBreaker] Circuit is half-open. Trying the next call...");
                        }
                   ).WithPolicyKey("CircuitBreakerPolicy");

            var executePolicy = retryPolicy.WrapAsync(circuitBreakerPolicy);
            return executePolicy;
        }

        public static AsyncPolicyWrap<IRestResponse> BuildPolicy(PolicyOptions options = null)
        {
            if (options == null)
                options = new PolicyOptions();

            var retryPolicy = Policy.
                HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.GatewayTimeout ||
                                                    r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                    r.StatusCode == HttpStatusCode.RequestTimeout ||
                                                    r.StatusCode == HttpStatusCode.BadGateway ||
                                                    r.StatusCode == (HttpStatusCode)429 ||
                                                    r.ResponseStatus == ResponseStatus.TimedOut ||
                                                    r.ResponseStatus == ResponseStatus.Error)
                .WaitAndRetryAsync(options.RetryOptions.Retries, retryAttemp => TimeSpan.FromSeconds(Math.Pow(options.RetryOptions.TimeBetweenRetries, retryAttemp)), onRetry: (response, delay, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    if (!string.IsNullOrWhiteSpace(response.Result?.ErrorMessage))
                        logger.LogError(response.Exception, $"[ExecuteRequest][Retry][{response.Result?.ErrorMessage}][Attempt {retryCount} of {options.RetryOptions.Retries}]");
                    else
                        logger.LogError(response.Exception, $"[ExecuteRequest][Retry][Failed with StatusCode: {response.Result?.StatusCode.ToString()}][Attempt {retryCount} of {options.RetryOptions.Retries}]");
                })
                .WithPolicyKey("RetryPolicy");

            //var circuitBreakerPolicy = Policy
            //       .HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.GatewayTimeout ||
            //                                       r.StatusCode == HttpStatusCode.ServiceUnavailable ||
            //                                       r.StatusCode == HttpStatusCode.RequestTimeout ||
            //                                       r.StatusCode == HttpStatusCode.BadGateway ||
            //                                       r.StatusCode == (HttpStatusCode)429 ||
            //                                       r.ResponseStatus == ResponseStatus.TimedOut ||
            //                                       r.ResponseStatus == ResponseStatus.Error)
            //       .CircuitBreakerAsync(
            //           handledEventsAllowedBeforeBreaking: options.ExceptionsAllowedBeforBreak,
            //           durationOfBreak: options.BreakDuration,
            //           onBreak: (response, delay, context) =>
            //           {
            //               var logger = context.GetLogger();
            //               logger.LogError($"[ExecuteRequest][CircuitBreaker] Breaking the circuit for {delay.TotalMilliseconds}ms due to {response.Result?.ErrorMessage}");
            //           },
            //           onReset: (context) =>
            //           {
            //               var logger = context.GetLogger();
            //               logger.LogInformation($"[ExecuteRequest][CircuitBreaker] Circuit is closed now");
            //           },
            //           onHalfOpen: () =>
            //           {
            //               Trace.WriteLine($"[ExecuteRequest][CircuitBreaker] Circuit is half-open. Trying the next call...");
            //           }
            //       ).WithPolicyKey("CircuitBreakerPolicy");

            var circuitBreakerPolicy = Policy
                   .HandleResult<IRestResponse>(r => r.StatusCode == HttpStatusCode.GatewayTimeout ||
                                                   r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                   r.StatusCode == HttpStatusCode.RequestTimeout ||
                                                   r.StatusCode == HttpStatusCode.BadGateway ||
                                                   r.StatusCode == (HttpStatusCode)429 ||
                                                   r.ResponseStatus == ResponseStatus.TimedOut ||
                                                   r.ResponseStatus == ResponseStatus.Error)
                   .AdvancedCircuitBreakerAsync(
                        failureThreshold: 0.5,
                        samplingDuration: TimeSpan.FromSeconds(60),
                        minimumThroughput: 10,
                        durationOfBreak: TimeSpan.FromSeconds(60),
                        onBreak: (response, delay, context) =>
                        {
                            var logger = context.GetLogger();
                            logger.LogError($"[ExecuteRequest][CircuitBreaker] Breaking the circuit for {delay.TotalMilliseconds}ms due to {response.Result?.ErrorMessage}");
                        },
                        onReset: (context) =>
                        {
                            var logger = context.GetLogger();
                            logger.LogInformation($"[ExecuteRequest][CircuitBreaker] Circuit is closed now");
                        },
                        onHalfOpen: () =>
                        {
                            Trace.WriteLine($"[ExecuteRequest][CircuitBreaker] Circuit is half-open. Trying the next call...");
                        }
                   ).WithPolicyKey("CircuitBreakerPolicy");

            var executePolicy = retryPolicy.WrapAsync(circuitBreakerPolicy);
            return executePolicy;
        }
    }
}
