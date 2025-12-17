using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ServiceDefaults.Middleware.Keycloak
{
    public static class KeycloakExtension
    {
        public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication()
                .AddKeycloakJwtBearer(serviceName: "keycloak", realm: "clothy-realm",
                    configureOptions: options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.Audience = "clothy-api";
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            RoleClaimType = ClaimTypes.Role
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
