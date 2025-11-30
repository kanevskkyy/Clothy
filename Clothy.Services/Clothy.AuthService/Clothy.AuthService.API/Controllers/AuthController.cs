using Clothy.AuthService.BLL.DTOs.Auth;
using Clothy.AuthService.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.AuthService.API.Controllers
{
    /// <summary>
    /// Controller for user authentication management
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private IKeycloakAuthService authService;
        private ILogger<AuthController> logger;

        public AuthController(IKeycloakAuthService authService, ILogger<AuthController> logger)
        {
            this.authService = authService;
            this.logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDTO">Registration data (Email, Password, FirstName, LastName, PhoneNumber)</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Registered user data</returns>
        /// <response code="200">User successfully registered</response>
        /// <response code="400">Error during registration</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Registration attempt for {Email}", registerDTO.Email);
            var user = await authService.RegisterUserAsync(registerDTO, cancellationToken);
            return Ok(user);
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="loginDTO">Login details (Email, Password)</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Access tokens (AccessToken, RefreshToken)</returns>
        /// <response code="200">Successful login</response>
        /// <response code="401">Invalid email or password</response>
        /// <response code="400">Login error</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Login attempt for {Email}", loginDTO.Email);
            var tokenResponse = await authService.LoginAsync(loginDTO, cancellationToken);
            return Ok(tokenResponse);
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshTokenDTO">Refresh token</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>New access tokens</returns>
        /// <response code="200">Token successfully refreshed</response>
        /// <response code="401">Invalid refresh token</response>
        /// <response code="400">Token refresh error</response>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Token refresh attempt");
            var tokenResponse = await authService.RefreshTokenAsync(refreshTokenDTO, cancellationToken);
            return Ok(tokenResponse);
        }

        /// <summary>
        /// User logout from the system
        /// </summary>
        /// <param name="refreshTokenDTO">Refresh token</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Logout confirmation</returns>
        /// <response code="200">User successfully logged out</response>
        /// <response code="400">Logout error</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO refreshTokenDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Logout attempt");
            await authService.LogoutAsync(refreshTokenDTO.RefreshToken, cancellationToken);
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        /// <param name="forgotPasswordDTO">User's email address</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Email sending confirmation</returns>
        /// <response code="200">Password reset email sent</response>
        /// <response code="400">Error sending email</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Forgot password request for {Email}", forgotPasswordDTO.Email);
            await authService.SendPasswordResetEmailAsync(forgotPasswordDTO, cancellationToken);
            return Ok(new { message = "Password reset email sent" });
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        /// <param name="resetPasswordDTO">Data for resetting the password (CurrentPassword, NewPassword)</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Confirmation of password change</returns>
        /// <response code="200">Password successfully changed</response>
        /// <response code="401">Incorrect current password</response>
        /// <response code="400">Error changing password</response>
        [HttpPost("reset-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO, CancellationToken cancellationToken)
        {
            logger.LogInformation("Password reset attempt");
            await authService.ResetPasswordAsync(resetPasswordDTO, User, cancellationToken);
            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Resend email for verification
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="cancellationToken">Operation cancellation token</param>
        /// <returns>Confirmation of email sending</returns>
        /// <response code="200">Verification email sent</response>
        /// <response code="400">Error sending email</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerificationEmail([FromQuery] string email, CancellationToken cancellationToken)
        {
            logger.LogInformation("Resend verification request for {Email}", email);
            await authService.ResendVerificationEmailAsync(email, cancellationToken);
            return Ok(new { message = "Verification email sent" });
        }
    }
}