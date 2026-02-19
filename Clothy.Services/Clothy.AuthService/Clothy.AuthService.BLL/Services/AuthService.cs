using AutoMapper;
using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.DTOs.Auth;
using Clothy.AuthService.BLL.DTOs.Users;
using Clothy.AuthService.BLL.Services.Interfaces;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.Services
{
    public class AuthService : IKeycloakAuthService
    {
        private HttpClient httpClient;
        private ILogger<AuthService> logger;
        private IUserClaimsExtractor userClaimsExtractor;
        private KeycloakSettings keycloakSettings;
        private IMapper mapper;
        private IKeycloakUserHelper keycloakUserHelper;
        private IEntityCacheService cacheService;
        private const string ADMIN_TOKEN_CACHE_KEY = "kc_admin_token";

        private const string DEFAULT_PHOTO_URL = "https://res.cloudinary.com/dkdljnfja/image/upload/v1763818143/Profile_Avatar_cfazhc.png";

        public AuthService(
            HttpClient httpClient,
            ILogger<AuthService> logger,
            IOptions<KeycloakSettings> keycloakOptions,
            IUserClaimsExtractor userClaimsExtractor,
            IMapper mapper,
            IKeycloakUserHelper keycloakUserHelper,
            IEntityCacheService entityCacheService)
        {
            this.userClaimsExtractor = userClaimsExtractor;
            this.httpClient = httpClient;
            this.logger = logger;
            this.cacheService = entityCacheService;
            this.mapper = mapper;
            keycloakSettings = keycloakOptions.Value;
            this.keycloakUserHelper = keycloakUserHelper;
        }

        public async Task<RegisterResponseDTO> RegisterUserAsync(RegisterDTO registerDTO, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting user registration for email: {Email}", registerDTO.Email);

            string adminToken = await GetAdminTokenAsync(cancellationToken);
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            object newUser = new
            {
                username = registerDTO.Email,
                email = registerDTO.Email,
                firstName = registerDTO.FirstName,
                lastName = registerDTO.LastName,
                enabled = true,
                emailVerified = false,
                attributes = new Dictionary<string, string[]>
                {
                    ["phoneNumber"] = new[] { registerDTO.PhoneNumber },
                    ["photoURL"] = new[] { DEFAULT_PHOTO_URL }
                },
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = registerDTO.Password,
                        temporary = false
                    }
                }
            };

            string json = JsonSerializer.Serialize(newUser);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Registration failed: {Error}", error);
                throw new Exception($"Registration failed: {error}");
            }

            string userId = await keycloakUserHelper.GetUserIdByEmailAsync(registerDTO.Email, adminToken, cancellationToken);
            await AssignRoleToUserAsync(userId, "User", adminToken, cancellationToken);
            await SendVerificationEmailAsync(userId, adminToken, cancellationToken);

            logger.LogInformation("User registered successfully with ID: {UserId}", userId);

            LoginResponseDTO loginResponse = await LoginAsync(new LoginDTO
            {
                Email = registerDTO.Email,
                Password = registerDTO.Password
            }, cancellationToken);

            return new RegisterResponseDTO
            {
                User = loginResponse.User,
                Tokens = loginResponse.Tokens
            };
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("User login attempt for email: {Email}", loginDTO.Email);

            string tokenUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/token";

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = keycloakSettings.ClientId,
                ["client_secret"] = keycloakSettings.ClientSecret,
                ["username"] = loginDTO.Email,
                ["password"] = loginDTO.Password,
                ["scope"] = "openid profile email"
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
            HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Login failed for email: {Email}. Error: {Error}", loginDTO.Email, error);
                throw new ValidationFailedException("Invalid email or password");
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            KeycloakTokenResponse keycloakResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json)!;

            TokenResponseDTO tokens = mapper.Map<TokenResponseDTO>(keycloakResponse);

            string adminToken = await GetAdminTokenAsync(cancellationToken);
            UserReadDTO user = await keycloakUserHelper.GetUserByEmailAsync(loginDTO.Email, adminToken, cancellationToken);

            logger.LogInformation("User logged in successfully: {Email}", loginDTO.Email);

            return new LoginResponseDTO
            {
                Tokens = tokens,
                User = user
            };
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenDTO refreshTokenDTO, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Refreshing token");

            string tokenUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/token";

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = keycloakSettings.ClientId,
                ["client_secret"] = keycloakSettings.ClientSecret,
                ["refresh_token"] = refreshTokenDTO.RefreshToken
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
            HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Token refresh failed. Error: {Error}", error);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            KeycloakTokenResponse keycloakResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(json)!;

            return mapper.Map<TokenResponseDTO>(keycloakResponse);
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Logging out user");

            string logoutUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/logout";

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                ["client_id"] = keycloakSettings.ClientId,
                ["client_secret"] = keycloakSettings.ClientSecret,
                ["refresh_token"] = refreshToken
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
            HttpResponseMessage response = await httpClient.PostAsync(logoutUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("Logout failed. Error: {Error}", error);
            }

            logger.LogInformation("User logged out successfully");
        }

        public async Task SendPasswordResetEmailAsync(ForgotPasswordDTO forgotPasswordDTO, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Password reset requested for email: {Email}", forgotPasswordDTO.Email);

            string adminToken = await GetAdminTokenAsync(cancellationToken);
            string userId = await keycloakUserHelper.GetUserIdByEmailAsync(forgotPasswordDTO.Email, adminToken, cancellationToken);

            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("User not found for password reset: {Email}", forgotPasswordDTO.Email);
                return;
            }

            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}/execute-actions-email";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            string[] actions = new[] { "UPDATE_PASSWORD" };
            string json = JsonSerializer.Serialize(actions);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to send password reset email: {Error}", error);
                throw new Exception($"Failed to send password reset email: {error}");
            }

            logger.LogInformation("Password reset email sent to: {Email}", forgotPasswordDTO.Email);
        }

        public async Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            string userId = userClaimsExtractor.GetUserId(user).ToString();
            string email = userClaimsExtractor.GetEmail(user);

            logger.LogInformation("Password change requested for user: {UserId}", userId);

            try
            {
                await LoginAsync(new LoginDTO { Email = email, Password = resetPasswordDTO.CurrentPassword }, cancellationToken);
            }
            catch
            {
                logger.LogWarning("Invalid current password for user: {UserId}", userId);
                throw new ValidationFailedException("Current password is incorrect");
            }

            string adminToken = await GetAdminTokenAsync(cancellationToken);
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}/reset-password";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            object passwordData = new
            {
                type = "password",
                value = resetPasswordDTO.NewPassword,
                temporary = false
            };

            string json = JsonSerializer.Serialize(passwordData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to reset password: {Error}", error);
                throw new Exception($"Failed to reset password: {error}");
            }

            logger.LogInformation("Password changed successfully for user: {UserId}", userId);
        }

        public async Task ResendVerificationEmailAsync(ResendVerificationEmailDTO resendVerificationEmailDTO, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Resending verification email to: {Email}", resendVerificationEmailDTO.Email);

            string adminToken = await GetAdminTokenAsync(cancellationToken);
            string userId = await keycloakUserHelper.GetUserIdByEmailAsync(resendVerificationEmailDTO.Email, adminToken, cancellationToken);

            if (string.IsNullOrEmpty(userId)) throw new NotFoundException("User not found");

            await SendVerificationEmailAsync(userId, adminToken, cancellationToken);

            logger.LogInformation("Verification email resent to: {Email}", resendVerificationEmailDTO.Email);
        }

        private async Task<string?> GetAdminTokenAsync(CancellationToken cancellationToken = default)
        {
            return await cacheService.GetOrSetAsync<string>(
                ADMIN_TOKEN_CACHE_KEY,
                async () =>
                {
                    string tokenUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/token";

                    httpClient.DefaultRequestHeaders.Authorization = null;

                    Dictionary<string, string> formData = new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials",
                        ["client_id"] = keycloakSettings.ClientId!,
                        ["client_secret"] = keycloakSettings.ClientSecret!
                    };

                    FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
                    HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync(cancellationToken);
                    JsonElement tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);

                    return tokenResponse.GetProperty("access_token").GetString();
                },
                memoryExpiration: TimeSpan.FromSeconds(55),
                redisExpiration: TimeSpan.FromSeconds(55)
            );
        }

        private async Task SendVerificationEmailAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}/send-verify-email";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.PutAsync(url, null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        private async Task AssignRoleToUserAsync(string userId, string roleName, string adminToken, CancellationToken cancellationToken = default)
        {
            string roleUrl = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/roles/{roleName}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            HttpResponseMessage roleResponse = await httpClient.GetAsync(roleUrl, cancellationToken);
            roleResponse.EnsureSuccessStatusCode();

            string roleJson = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
            JsonElement role = JsonSerializer.Deserialize<JsonElement>(roleJson);

            string assignUrl = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}/role-mappings/realm";

            object[] roles = new[] { new { id = role.GetProperty("id").GetString(), name = roleName } };

            string json = JsonSerializer.Serialize(roles);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(assignUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}