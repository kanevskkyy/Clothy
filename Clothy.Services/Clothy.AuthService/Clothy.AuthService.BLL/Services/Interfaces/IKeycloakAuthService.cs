using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.AuthService.BLL.DTOs.Auth;
using Clothy.AuthService.BLL.DTOs.Users;

namespace Clothy.AuthService.BLL.Services.Interfaces
{
    public interface IKeycloakAuthService
    {
        Task<RegisterResponseDTO> RegisterUserAsync(RegisterDTO registerDTO, CancellationToken cancellationToken = default);
        Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO, CancellationToken cancellationToken = default);
        Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenDTO refreshTokenDTO, CancellationToken cancellationToken = default);
        Task SendPasswordResetEmailAsync(ForgotPasswordDTO forgotPasswordDTO, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO, ClaimsPrincipal user, CancellationToken cancellationToken = default);
        Task ResendVerificationEmailAsync(ResendVerificationEmailDTO resendVerificationEmailDTO, CancellationToken cancellationToken = default);
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
