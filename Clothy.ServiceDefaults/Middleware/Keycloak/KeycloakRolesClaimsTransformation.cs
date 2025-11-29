using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Clothy.ServiceDefaults.Middleware.Keycloak
{
    public class KeycloakRolesClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity!;

            if (!claimsIdentity.IsAuthenticated) return Task.FromResult(principal);
            
            var rolesClaim = principal.FindFirst("roles");
            if (rolesClaim != null && rolesClaim.Value.StartsWith("["))
            {
                try
                {
                    var rolesArray = JsonSerializer.Deserialize<string[]>(rolesClaim.Value);
                    claimsIdentity.RemoveClaim(rolesClaim);

                    foreach (var role in rolesArray ?? Array.Empty<string>())
                    {
                        if (!string.IsNullOrEmpty(role) && !role.StartsWith("role"))
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                    }
                }
                catch { }
            }

            return Task.FromResult(principal);
        }
    }
}
