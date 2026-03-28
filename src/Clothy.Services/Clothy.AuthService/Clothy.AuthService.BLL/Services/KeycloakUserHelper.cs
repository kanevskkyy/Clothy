using Clothy.AuthService.BLL.Config;
using Clothy.AuthService.BLL.DTOs.Users;
using Clothy.AuthService.BLL.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.Services
{
    public class KeycloakUserHelper : IKeycloakUserHelper
    {
        private HttpClient httpClient;
        private KeycloakSettings keycloakSettings;
        private const string DEFAULT_PHOTO_URL = "https://res.cloudinary.com/dkdljnfja/image/upload/v1763818143/Profile_Avatar_cfazhc.png";

        public KeycloakUserHelper(HttpClient httpClient, IOptions<KeycloakSettings> keycloakOptions)
        {
            this.httpClient = httpClient;
            this.keycloakSettings = keycloakOptions.Value;
        }

        public async Task<UserReadDTO> GetUserByEmailAsync(string email, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users?email={email}&exact=true";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement[]? users = JsonSerializer.Deserialize<JsonElement[]>(json);

            if (users == null || users.Length == 0) throw new NotFoundException("User not found");

            JsonElement keycloakUser = users[0];
            string userId = keycloakUser.GetProperty("id").GetString()!;

            List<string> roles = await GetUserRolesAsync(userId, adminToken, cancellationToken);

            return MapKeycloakUserToDTO(keycloakUser, roles);
        }

        public async Task<UserReadDTO> GetUserByIdAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement keycloakUser = JsonSerializer.Deserialize<JsonElement>(json);

            List<string> roles = await GetUserRolesAsync(userId, adminToken, cancellationToken);

            return MapKeycloakUserToDTO(keycloakUser, roles);
        }

        public async Task<string> GetUserIdByEmailAsync(string email, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users?email={email}&exact=true";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement[]? users = JsonSerializer.Deserialize<JsonElement[]>(json);

            if (users == null || users.Length == 0) return string.Empty;

            return users[0].GetProperty("id").GetString()!;
        }

        private async Task<List<string>> GetUserRolesAsync(string userId, string adminToken, CancellationToken cancellationToken = default)
        {
            string url = $"{keycloakSettings.Url}/admin/realms/{keycloakSettings.Realm}/users/{userId}/role-mappings/realm";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            JsonElement[]? roles = JsonSerializer.Deserialize<JsonElement[]>(json);

            return roles?.Select(r => r.GetProperty("name").GetString()!)
                        .Where(name => !name.StartsWith("default-") &&
                                     !name.StartsWith("offline_") &&
                                     !name.StartsWith("uma_"))
                        .ToList() ?? new List<string>();
        }

        private UserReadDTO MapKeycloakUserToDTO(JsonElement keycloakUser, List<string> roles)
        {
            string userId = keycloakUser.GetProperty("id").GetString()!;
            string email = keycloakUser.GetProperty("email").GetString()!;
            string firstName = keycloakUser.TryGetProperty("firstName", out JsonElement fn) ? fn.GetString() ?? string.Empty : string.Empty;
            string lastName = keycloakUser.TryGetProperty("lastName", out JsonElement ln) ? ln.GetString() ?? string.Empty : string.Empty;
            bool emailVerified = keycloakUser.GetProperty("emailVerified").GetBoolean();

            string phoneNumber = string.Empty;
            string photoUrl = DEFAULT_PHOTO_URL;

            if (keycloakUser.TryGetProperty("attributes", out JsonElement attrs))
            {
                if (attrs.TryGetProperty("phoneNumber", out JsonElement phoneArray) && phoneArray.GetArrayLength() > 0) phoneNumber = phoneArray[0].GetString() ?? string.Empty;
                if (attrs.TryGetProperty("photoURL", out JsonElement photoArray) && photoArray.GetArrayLength() > 0) photoUrl = photoArray[0].GetString() ?? DEFAULT_PHOTO_URL;
            }

            return new UserReadDTO
            {
                Id = Guid.Parse(userId),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                PhotoUrl = photoUrl,
                EmailVerified = emailVerified,
                Roles = roles
            };
        }
    }
}
