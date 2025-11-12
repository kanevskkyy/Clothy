using Clothy.ServiceDefaults.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Events;
using System.Diagnostics;
using OpenTelemetry.Resources;
using Clothy.Shared.Cache;
using Clothy.Shared.Cache.Interfaces;
using StackExchange.Redis;
using System.Text.Json;
using System.Reflection;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddSerilog((ctx, lc) => lc
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", builder.Environment.ApplicationName)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console()
        );

        // REDIS CONFIGURATION
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            string redisHost = "clothy-redis";
            int redisPort = 6379;

            ConfigurationOptions config = ConfigurationOptions.Parse($"{redisHost}:{redisPort}");
            config.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(config);
        });

        builder.Services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024;

            options.CompactionPercentage = 0.2;
        });
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IEntityCacheService, EntityCacheService>();

        builder.Services.AddHealthChecks();
        //

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
            http.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        });

        builder.Services.AddSwaggerGen(options =>
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                string xmlFilename = $"{entryAssembly.GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

        return builder;
    }

    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        app.UseCorrelationId();
        app.UseServiceLogging();
        app.MapDefaultEndpoints();

        return app;
    }

    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        ((IHostApplicationBuilder)builder).AddServiceDefaults();
        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(builder.Environment.ApplicationName))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation();

                var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otlpEndpoint);
                        otlpOptions.BatchExportProcessorOptions = new BatchExportActivityProcessorOptions
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512
                        };
                    });
                }
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("ServiceName", builder.Environment.ApplicationName);
                        activity.SetTag("Environment", builder.Environment.EnvironmentName);
                        activity.SetTag("http.scheme", request.Scheme);
                        activity.SetTag("http.host", request.Host.Host);
                    };

                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.status_code", response.StatusCode);
                    };

                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("error", true);
                        activity.SetTag("exception.type", exception.GetType().Name);
                        activity.SetTag("exception.message", exception.Message);
                    };
                });

                tracing.AddHttpClientInstrumentation(options =>
                {
                    options.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("http.url", request.RequestUri?.ToString());
                    };

                    options.EnrichWithHttpResponseMessage = (activity, response) =>
                    {
                        activity.SetTag("http.status_code", (int)response.StatusCode);
                    };

                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("error", true);
                        activity.SetTag("exception.type", exception.GetType().Name);
                        activity.SetTag("exception.message", exception.Message);
                    };
                });

                tracing.AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.statement", command.CommandText);
                    };
                });

                tracing.AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");

                tracing.SetSampler(builder.Environment.IsDevelopment()
                    ? new AlwaysOnSampler()
                    : new TraceIdRatioBasedSampler(0.25));

                if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
                {
                    tracing.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
                        otlpOptions.BatchExportProcessorOptions = new BatchExportActivityProcessorOptions
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512
                        };
                    });
                }
            });

        return builder;
    }


    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}