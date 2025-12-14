using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.DTOs.Users;
using Clothy.AuthService.BLL.Services.Interfaces;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Clothy.Shared.Helpers.JWT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clothy.Shared.Events.UserEvents;

namespace Clothy.AuthService.BLL.Services
{
    public class UserService : IUserService
    {
        private HttpClient httpClient;
        private IImageService imageService;
        private IKeycloakAuthService keycloakAuthService;
        private ILogger<UserService> logger;
        private IPublishEndpoint publishEndpoint;
        private IUserClaimsExtractor userClaimsExtractor;
        private KeycloakSettings keycloakSettings;
        private const string DEFAULT_PHOTO_URL = "https://res.cloudinary.com/dkdljnfja/image/upload/v1763818143/Profile_Avatar_cfazhc.png";

        public UserService(
            IUserClaimsExtractor userClaimsExtractor,
            HttpClient httpClient,
            IImageService imageService,
            IKeycloakAuthService keycloakAuthService,
            ILogger<UserService> logger,
            IOptions<KeycloakSettings> keycloakOptions,
            IPublishEndpoint publishEndpoint)
        {
            this.userClaimsExtractor = userClaimsExtractor;
            this.httpClient = httpClient;
            this.imageService = imageService;
            this.keycloakAuthService = keycloakAuthService;
            this.logger = logger;
            keycloakSettings = keycloakOptions.Value;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task<UserReadDTO> GetCurrentUserAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            string userId = userClaimsExtractor.GetUserId(user).ToString();
            string adminToken = await GetAdminTokenAsync(cancellationToken);

            JsonElement userData = await GetUserByIdAsync(userId, adminToken, cancellationToken);

            JsonElement attributes = userData.GetProperty("attributes");
            string? phoneNumber = attributes.TryGetProperty("phoneNumber", out var phone) ? phone[0].GetString() : string.Empty;
            string? photoUrl = attributes.TryGetProperty("photoURL", out var photo) ? photo[0].GetString() : DEFAULT_PHOTO_URL;

            List<string> roles = await GetUserRolesAsync(userId, adminToken, cancellationToken);

            return new UserReadDTO
            {
                Id = Guid.Parse(userId),
                Email = userData.GetProperty("email").GetString()!,
                FirstName = userData.GetProperty("firstName").GetString()!,
                LastName = userData.GetProperty("lastName").GetString()!,
                PhoneNumber = phoneNumber ?? string.Empty,
                PhotoUrl = photoUrl ?? DEFAULT_PHOTO_URL,
                EmailVerified = userData.GetProperty("emailVerified").GetBoolean(),
                Roles = roles
            };
        }

        public async Task<UserReadDTO> UpdateUserAsync(UserUpdateDTO userUpdateDTO, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            string userId = userClaimsExtractor.GetUserId(user).ToString();
            logger.LogInformation("Updating user: {UserId}", userId);

            string adminToken = await GetAdminTokenAsync(cancellationToken);
            var currentUser = await GetUserByIdAsync(userId, adminToken, cancellationToken);

            string? newPhotoUrl = null;
            if (userUpdateDTO.Photo != null)
            {
                logger.LogInformation("Uploading new photo for user: {UserId}", userId);
                newPhotoUrl = await imageService.UploadAsync(userUpdateDTO.Photo, "users");

                string? oldPhotoUrl = currentUser.GetProperty("attributes").GetProperty("photoURL")[0].GetString();
                if (!string.IsNullOrEmpty(oldPhotoUrl) && !oldPhotoUrl.Contains("Profile_Avatar_cfazhc.png")) await imageService.DeleteImageAsync(oldPhotoUrl);
            }

            Dictionary<string, object> updateData = new Dictionary<string, object>();

            updateData["firstName"] = userUpdateDTO.FirstName;
            updateData["lastName"] = userUpdateDTO.LastName;

            Dictionary<string, string[]> attributes = new Dictionary<string, string[]>();

            attributes["phoneNumber"] = new[] { 
                userUpdateDTO.PhoneNumber 
            };

            if (newPhotoUrl != null) attributes["photoURL"] = new[] { newPhotoUrl };

            if (attributes.Count > 0) updateData["attributes"] = attributes;

            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            string json = JsonSerializer.Serialize(updateData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string? error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to update user: {Error}", error);
                throw new Exception($"Failed to update user: {error}");
            }

            logger.LogInformation("User updated successfully: {UserId}", userId);

            UserUpdatedEvent userUpdatedEvent = new UserUpdatedEvent() 
            {
                FirstName = userUpdateDTO.FirstName,
                LastName = userUpdateDTO.LastName,
                PhotoUrl = newPhotoUrl
            };
            await publishEndpoint.Publish(userUpdatedEvent, cancellationToken);

            return await GetCurrentUserAsync(user, cancellationToken);
        }

        private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken = default)
        {
            string tokenUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/token";

            httpClient.DefaultRequestHeaders.Authorization = null;

            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = keycloakSettings.ClientId,
                ["client_secret"] = keycloakSettings.ClientSecret
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
            HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
            return tokenResponse.GetProperty("access_token").GetString()!;
        }

        private async Task<JsonElement> GetUserByIdAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private async Task<List<string>> GetUserRolesAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}/role-mappings/realm";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement[]? roles = JsonSerializer.Deserialize<JsonElement[]>(json);

            return roles?.Select(r => r.GetProperty("name").GetString()!).ToList() ?? new List<string>();
        }
    }
}
