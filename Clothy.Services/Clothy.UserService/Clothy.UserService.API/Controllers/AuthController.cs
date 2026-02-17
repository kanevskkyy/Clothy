using Clothy.UserService.BLL.DTOs.AuthDTOs;
using Clothy.UserService.BLL.DTOs.TokenDTOs;
using Clothy.UserService.BLL.Services.Interfaces;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clothy.UserService.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService authService;
        private ITokenService tokenService;
        private ILogger<AuthController> logger;

        public AuthController(IAuthService authService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            this.authService = authService;
            this.tokenService = tokenService;
            this.logger = logger;
        }

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="registerDTO">User registration data.</param>
        /// <returns>JWT access and refresh tokens.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            logger.LogInformation("Registering user with email: {Email}", registerDTO.Email);
            TokenResponseDTO tokens = await authService.RegisterUserAsync(registerDTO);

            logger.LogInformation("User registered with email: {Email}", registerDTO.Email);
            return Ok(tokens);
        }

        /// <summary>
        /// Login user with email and password.
        /// </summary>
        /// <param name="loginDTO">User login data.</param>
        /// <returns>JWT access and refresh tokens.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            logger.LogInformation("Login attempt for email: {Email}", loginDTO.Email);
            TokenResponseDTO tokens = await authService.LoginAsync(loginDTO);

            logger.LogInformation("Login successful for email: {Email}", loginDTO.Email);
            return Ok(tokens);
        }

        /// <summary>
        /// Refresh access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshTokenDTO">Refresh token DTO.</param>
        /// <returns>New access and refresh tokens.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            logger.LogInformation("Refreshing token.");
            TokenResponseDTO newTokens = await tokenService.RefreshAccessTokenAsync(refreshTokenDTO);

            logger.LogInformation("Token refreshed successfully.");
            return Ok(newTokens);
        }

        /// <summary>
        /// Revoke a refresh token (logout).
        /// </summary>
        /// <param name="refreshTokenDTO">Refresh token DTO.</param>
        /// <returns>No content if successful.</returns>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            logger.LogInformation("Revoking refresh token for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await tokenService.RevokeRefreshTokenAsync(refreshTokenDTO);

            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to revoke refresh token for user {UserId}. Errors: {Errors}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, result.Errors);
                return BadRequest(result.Errors);
            }

            logger.LogInformation("Refresh token revoked successfully for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return NoContent();
        }

        /// <summary>
        /// Change password for the authenticated user.
        /// </summary>
        /// <param name="changePasswordDTO">Old and new password.</param>
        /// <returns>No content if successful.</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            logger.LogInformation("User {UserId} requested password change", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var result = await authService.ChangePasswordAsync(changePasswordDTO, User);

            if (!result.Succeeded)
            {
                logger.LogWarning("Failed password change for user {UserId}: {Errors}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, result.Errors);
                return BadRequest(result.Errors);
            }

            logger.LogInformation("Password changed successfully for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return NoContent();
        }
    }
}
