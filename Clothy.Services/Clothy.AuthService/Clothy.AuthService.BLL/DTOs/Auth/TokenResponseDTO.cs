using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.DTOs.Auth
{
    public class TokenResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public int RefreshExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public string? SessionState { get; set; }
        public string Scope { get; set; } = string.Empty;

        public static TokenResponseDTO FromKeycloakResponse(KeycloakTokenResponse keycloakResponse)
        {
            return new TokenResponseDTO
            {
                AccessToken = keycloakResponse.AccessToken,
                ExpiresIn = keycloakResponse.ExpiresIn,
                RefreshExpiresIn = keycloakResponse.RefreshExpiresIn,
                RefreshToken = keycloakResponse.RefreshToken,
                TokenType = keycloakResponse.TokenType,
                SessionState = keycloakResponse.SessionState,
                Scope = keycloakResponse.Scope
            };
        }
    }
}