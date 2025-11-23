using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.TokenDTOs;
using Clothy.UserService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Clothy.UserService.BLL.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponseDTO> GenerateTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default);
        Task<TokenResponseDTO> RefreshAccessTokenAsync(RefreshTokenDTO refreshToken, CancellationToken cancellationToken = default);
        Task<IdentityResult> RevokeRefreshTokenAsync(RefreshTokenDTO refreshToken, CancellationToken cancellationToken = default);
    }
}
