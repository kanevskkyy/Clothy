using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Clothy.ServiceDefaults.Middleware.Keycloak
{
    public static class KeycloakExtension
    {
        private static ConfigurationManager<OpenIdConnectConfiguration>? configurationManager;
        private static readonly object _lock = new object();

        public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddKeycloakJwtBearer(serviceName: "keycloak", realm: "clothy-realm",
                    configureOptions: options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.Audience = "clothy-api";
                        options.SaveToken = false;

                        string keycloakUrl = Environment.GetEnvironmentVariable("KEYCLOAK__URL") ?? "http://localhost:8080";
                        options.MetadataAddress = $"{keycloakUrl}/realms/clothy-realm/.well-known/openid-configuration";

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            RoleClaimType = ClaimTypes.Role,
                            ValidateIssuerSigningKey = true,
                            ValidateIssuer = true,
                            ValidIssuers = new[]
                            {
                                "http://localhost:8080/realms/clothy-realm",
                                "http://keycloak:8080/realms/clothy-realm"
                            },
                            ValidateAudience = true,
                            ValidAudiences = new[] { "clothy-api" },
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            ValidateActor = false,
                            ValidateTokenReplay = false
                        };

                        options.RefreshOnIssuerKeyNotFound = true;

                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                string? token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

                                if (string.IsNullOrEmpty(token))
                                {
                                    context.NoResult();
                                    return Task.CompletedTask;
                                }

                                if (configurationManager == null && !string.IsNullOrEmpty(context.Options.MetadataAddress))
                                {
                                    lock (_lock)
                                    {
                                        if (configurationManager == null)
                                        {
                                            configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                                                context.Options.MetadataAddress,
                                                new OpenIdConnectConfigurationRetriever(),
                                                new HttpDocumentRetriever()
                                                {
                                                    RequireHttps = context.Options.RequireHttpsMetadata
                                                })
                                            {
                                                AutomaticRefreshInterval = TimeSpan.FromHours(24),
                                                RefreshInterval = TimeSpan.FromHours(1)
                                            };
                                        }
                                    }
                                }

                                context.Options.ConfigurationManager = configurationManager;

                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("ManagerOrAdmin", policy =>
                    policy.RequireRole("Admin", "Manager"));
            });

            return services;
        }
    }
}