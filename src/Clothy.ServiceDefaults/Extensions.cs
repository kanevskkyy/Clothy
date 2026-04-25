using Clothy.ServiceDefaults.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using OpenTelemetry.Resources;
using System.Text.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Clothy.Shared.Helpers.JWT;
using Microsoft.AspNetCore.Authentication;
using Clothy.ServiceDefaults.Middleware.CorrelationId;
using Clothy.ServiceDefaults.Middleware.Keycloak;
using Clothy.ServiceDefaults.Middleware.Logging;
using Clothy.ServiceDefaults.Middleware.JWT;
using Clothy.ServiceDefaults.Middleware.Redis;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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

        var allowedOrigins = (Environment.GetEnvironmentVariable("FRONTEND__URL") ?? "http://localhost:5173").Split(',');

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddMemoryCache();
        builder.Services.AddCaching();

        builder.Services.AddHealthChecks();

        builder.AddRabbitMQClient(connectionName: "rabbitmq");
        builder.Services.AddHealthChecks()
            .AddRabbitMQ(
                name: "rabbitmq",
                tags: new[] { "ready" }
            );

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
            http.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        });

        builder.Services.AddSwaggerWithXmlComments();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

        // OLD USER MICROSERVICE

        //builder.Services.AddJwtAuthentication(builder.Configuration);

        //

        builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
        builder.Services.AddKeycloakAuthentication();

        builder.Services.AddSwaggerWithAuth();
        builder.Services.AddScoped<IUserClaimsExtractor, UserClaimsExtractor>();

        return builder;
    }

    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        app.UseCors();
        app.UseCorrelationId();
        app.UseServiceLogging();
        app.MapDefaultEndpoints();
        
        app.MapControllers();

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
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = hc => hc.Tags.Contains("live")
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = hc => hc.Tags.Contains("ready"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description
                        })
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                }
            });
        }

        return app;
    }
}