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
using Clothy.Shared.Helpers.JWT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clothy.Shared.Events.UserEvents;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;

namespace Clothy.AuthService.BLL.Services
{
    public class UserService : IUserService
    {
        private HttpClient httpClient;
        private IImageService imageService;
        private ILogger<UserService> logger;
        private IPublishEndpoint publishEndpoint;
        private IUserClaimsExtractor userClaimsExtractor;
        private KeycloakSettings keycloakSettings;
        private IKeycloakUserHelper keycloakUserHelper;
        public UserService(
            IUserClaimsExtractor userClaimsExtractor,
            HttpClient httpClient,
            IImageService imageService,
            ILogger<UserService> logger,
            IOptions<KeycloakSettings> keycloakOptions,
            IPublishEndpoint publishEndpoint,
            IKeycloakUserHelper keycloakUserHelper)
        {
            this.userClaimsExtractor = userClaimsExtractor;
            this.httpClient = httpClient;
            this.imageService = imageService;
            this.logger = logger;
            this.keycloakSettings = keycloakOptions.Value;
            this.publishEndpoint = publishEndpoint;
            this.keycloakUserHelper = keycloakUserHelper;
        }

        public async Task<UserReadDTO> GetCurrentUserAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            string userId = userClaimsExtractor.GetUserId(user).ToString();
            string adminToken = await GetAdminTokenAsync(cancellationToken);

            return await keycloakUserHelper.GetUserByIdAsync(userId, adminToken, cancellationToken);
        }

        public async Task<UserReadDTO> UpdateUserAsync(UserUpdateDTO userUpdateDTO, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            string userId = userClaimsExtractor.GetUserId(user).ToString();
            logger.LogInformation("Updating user: {UserId}", userId);

            string adminToken = await GetAdminTokenAsync(cancellationToken);

            UserReadDTO currentUser = await keycloakUserHelper.GetUserByIdAsync(userId, adminToken, cancellationToken);

            string? newPhotoUrl = null;
            if (userUpdateDTO.Photo != null)
            {
                logger.LogInformation("Uploading new photo for user: {UserId}", userId);
                newPhotoUrl = await imageService.UploadAsync(userUpdateDTO.Photo, "users");

                if (!string.IsNullOrEmpty(currentUser.PhotoUrl) && !currentUser.PhotoUrl.Contains("Profile_Avatar_cfazhc.png")) await imageService.DeleteImageAsync(currentUser.PhotoUrl);
            }

            Dictionary<string, object> updateData = new Dictionary<string, object>
            {
                ["firstName"] = userUpdateDTO.FirstName!,
                ["lastName"] = userUpdateDTO.LastName!
            };

            Dictionary<string, string[]> attributes = new Dictionary<string, string[]>
            {
                ["phoneNumber"] = new[] { userUpdateDTO.PhoneNumber! }
            };

            if (newPhotoUrl != null) attributes["photoURL"] = new[] { newPhotoUrl };

            updateData["attributes"] = attributes;

            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            string json = JsonSerializer.Serialize(updateData);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to update user: {Error}", error);
                throw new Exception($"Failed to update user: {error}");
            }

            logger.LogInformation("User updated successfully: {UserId}", userId);

            UserUpdatedEvent userUpdatedEvent = new UserUpdatedEvent
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
                ["client_id"] = keycloakSettings.ClientId!,
                ["client_secret"] = keycloakSettings.ClientSecret!
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(formData);
            HttpResponseMessage response = await httpClient.PostAsync(tokenUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
            return tokenResponse.GetProperty("access_token").GetString()!;
        }
    }
}