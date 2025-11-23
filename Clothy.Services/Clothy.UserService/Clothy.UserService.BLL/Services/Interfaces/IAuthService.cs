using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.AuthDTOs;
using Clothy.UserService.BLL.DTOs.TokenDTOs;
using Microsoft.AspNetCore.Identity;

namespace Clothy.UserService.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponseDTO> RegisterUserAsync(RegisterDTO registerDTO, CancellationToken cancellationToken = default);
        Task<TokenResponseDTO> LoginAsync(LoginDTO loginDTO, CancellationToken cancellationToken = default);
        Task<IdentityResult> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO, ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default);
    }
}
