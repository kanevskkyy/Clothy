using Clothy.ServiceDefaults.Middleware.CorrelationId;
using Clothy.ServiceDefaults.Middleware.JWT;
using Clothy.ServiceDefaults.Middleware.Keycloak;
using Clothy.ServiceDefaults.Middleware.Redis;
using Clothy.Shared.Helpers.JWT;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.ServiceDefaults.Middleware.Routes
{
    public static class GatewayServiceDefaults
    {
        public static IHostApplicationBuilder AddGatewayDefaults(this IHostApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Services.AddSerilog((ctx, lc) => lc
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", builder.Environment.ApplicationName)
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Yarp", LogEventLevel.Information)
                .WriteTo.Console()
            );

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins(Environment.GetEnvironmentVariable("FRONTEND__URL") ?? "http://localhost:5173")
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

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
            });

            builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
            builder.Services.AddKeycloakAuthentication();
            builder.Services.AddSwaggerWithAuth();

            builder.Services.AddScoped<IUserClaimsExtractor, UserClaimsExtractor>();

            return builder;
        }

        public static WebApplication UseGatewayDefaults(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            return app;
        }
    }
}
