using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Clothy.ServiceDefaults.Middleware
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddConfiguredOpenTelemetry(this IServiceCollection services, string serviceName, IConfiguration configuration)
        {
            Meter meter = services.AddOrGetMeter(serviceName);

            services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                           .AddAspNetCoreInstrumentation() 
                           .AddRuntimeInstrumentation();  

                    string? otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        metrics.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(otlpEndpoint);
                        });
                    }

                    metrics.AddMeter(meter.Name);
                })
                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation(); 
                    tracing.AddEntityFrameworkCoreInstrumentation(); 

                    string? otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        tracing.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(otlpEndpoint);
                        });
                    }
                });

            return services;
        }

        public static Meter AddOrGetMeter(this IServiceCollection services, string serviceName)
        {
            Meter? existingMeter = services.FirstOrDefault(sd => sd.ServiceType == typeof(Meter))?.ImplementationInstance as Meter;

            if (existingMeter != null) return existingMeter;

            Meter meter = new Meter(serviceName);
            services.AddSingleton(meter);

            return meter;
        }
    }
}
