using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace Clothy.ServiceDefaults.Middleware
{
    public static class GrpcClientExtensions
    {
        public static IHttpClientBuilder AddConfiguredGrpcClient<TClient>(this IServiceCollection services, string serviceName) where TClient : class
        {
            var builder = services.AddGrpcClient<TClient>(options =>
                {
                    options.Address = new Uri($"https://{serviceName}");
                })
                .ConfigureChannel(channelOptions =>
                {
                    channelOptions.MaxReceiveMessageSize = 5 * 1024 * 1024;
                    channelOptions.MaxSendMessageSize = 5 * 1024 * 1024;
                })
                .AddServiceDiscovery();

            var serviceProvider = services.BuildServiceProvider();
            ILogger<TClient> logger = serviceProvider.GetRequiredService<ILogger<TClient>>();

            builder.AddStandardResilienceHandler(configure =>
            {
                configure.Retry = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(5),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = args =>
                    {
                        logger.LogWarning(
                            "Retry attempt {AttemptNumber} for {ServiceName}. " +
                            "Delay: {Delay}ms. Reason: {Outcome}. " +
                            "Operation: {Operation}",
                            args.AttemptNumber,
                            serviceName,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.GetType().Name ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown",
                            args.Outcome.Exception?.Message ?? "Request failed"
                        );
                        return ValueTask.CompletedTask;
                    }
                };

                configure.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 50,
                    BreakDuration = TimeSpan.FromSeconds(30),
                    SamplingDuration = TimeSpan.FromSeconds(60),
                    OnOpened = args =>
                    {
                        logger.LogError(
                            "Circuit breaker OPENED for {ServiceName}. " +
                            "Failure ratio exceeded threshold. " +
                            "Break duration: {BreakDuration}s. " +
                            "Reason: {Outcome}. " +
                            "State: {State}",
                            serviceName,
                            30,
                            args.Outcome.Exception?.GetType().Name ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown",
                            "Open"
                        );
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        logger.LogInformation(
                            "Circuit breaker CLOSED for {ServiceName}. " +
                            "Service recovered and accepting requests. " +
                            "State: {State}",
                            serviceName,
                            "Closed"
                        );
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        logger.LogWarning(
                            "Circuit breaker HALF-OPENED for {ServiceName}. " +
                            "Testing service availability. " +
                            "State: {State}",
                            serviceName,
                            "HalfOpen"
                        );
                        return ValueTask.CompletedTask;
                    }
                };

                configure.TotalRequestTimeout = new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(30),
                    OnTimeout = args =>
                    {
                        logger.LogError(
                            "Request TIMEOUT for {ServiceName}. " +
                            "Timeout threshold: {Timeout}s. " +
                            "Operation cancelled. " +
                            "Context: {Context}",
                            serviceName,
                            30,
                            args.Context.ToString()
                        );
                        return ValueTask.CompletedTask;
                    }
                };
            });

            return builder;
        }
    }
}