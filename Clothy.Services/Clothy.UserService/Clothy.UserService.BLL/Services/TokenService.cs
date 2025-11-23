using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Helpers.JWT;
using Clothy.UserService.BLL.DTOs.TokenDTOs;
using Clothy.UserService.BLL.Services.Interfaces;
using Clothy.UserService.DAL.Repositories.Interfaces;
using Clothy.UserService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clothy.UserService.BLL.Services
{
    public class TokenService : ITokenService
    {
        private UserManager<ApplicationUser> userManager;
        private IRefreshTokenRepository refreshTokenRepository;
        private JwtConfig jwtConfig;

        public TokenService(UserManager<ApplicationUser> userManager, IRefreshTokenRepository refreshTokenRepository, IOptions<JwtConfig> jwtOptions)
        {
            this.userManager = userManager;
            this.refreshTokenRepository = refreshTokenRepository;
            jwtConfig = jwtOptions.Value;

            Console.WriteLine("\n=== TokenService Constructor ===");
            Console.WriteLine($"Key: {jwtConfig.Key} (is null: {jwtConfig.Key == null})");
            Console.WriteLine($"Issuer: {jwtConfig.Issuer} (is null: {jwtConfig.Issuer == null})");
            Console.WriteLine($"Audience: {jwtConfig.Audience} (is null: {jwtConfig.Audience == null})");
            Console.WriteLine("=== END ===\n");
        }

        public async Task<TokenResponseDTO> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            string accessToken = await GenerateAccessTokenAsync(user);
            RefreshToken refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<TokenResponseDTO> RefreshAccessTokenAsync(RefreshTokenDTO refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshToken? token = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken.Token, cancellationToken);
            if (token == null || token.Revoked || token.ExpiresAt < DateTime.UtcNow.ToUniversalTime()) throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

            ApplicationUser? user = await userManager.FindByIdAsync(token.UserId.ToString());

            if (user == null) throw new UnauthorizedAccessException("User not found.");

            token.Revoked = true;
            await refreshTokenRepository.UpdateAsync(token, cancellationToken);

            string newAccessToken = await GenerateAccessTokenAsync(user);

            return new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = refreshToken.Token,
            };
        }

        public async Task<IdentityResult> RevokeRefreshTokenAsync(RefreshTokenDTO refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshToken? storedToken = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken.Token, cancellationToken);
            if (storedToken == null) return IdentityResult.Failed(new IdentityError { Description = "Refresh token not found." });

            storedToken.Revoked = true;
            await refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

            return IdentityResult.Success;
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("PhotoUrl", user.PhotoUrl),
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtConfig.AccessTokenDurationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            byte[] randomBytes = new byte[64];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            string token = Convert.ToBase64String(randomBytes);

            RefreshToken refreshToken = new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtConfig.RefreshTokenDurationDays),
                Revoked = false
            };
            return await refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
        }
    }
}
