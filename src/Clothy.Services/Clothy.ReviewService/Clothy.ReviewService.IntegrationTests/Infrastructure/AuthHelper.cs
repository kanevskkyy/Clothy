using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Clothy.ReviewService.IntegrationTests.Infrastructure;

public static class AuthHelper
{
    public static string GenerateJwtToken(Guid userId, string email = "test@test.com", string[] roles = null!)
    {
        roles ??= new[] { "User" };

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes("SuperSecretKeyForTestingPurposesOnly12345678"));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new()
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.GivenName, "Test"),
            new(ClaimTypes.Surname, "User"),
        };

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        JwtSecurityToken token = new(
            issuer: "ClothyTestIssuer",
            audience: "ClothyTestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static void AddAuthorizationHeader(this HttpClient client, Guid userId, string[] roles = null!)
    {
        string token = GenerateJwtToken(userId, roles: roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}