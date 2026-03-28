using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.AuthService.BLL.DTOs.Users;

namespace Clothy.AuthService.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserReadDTO> GetCurrentUserAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
        Task<UserReadDTO> UpdateUserAsync(UserUpdateDTO userUpdateDTO, ClaimsPrincipal user, CancellationToken cancellationToken = default);
    }
}
