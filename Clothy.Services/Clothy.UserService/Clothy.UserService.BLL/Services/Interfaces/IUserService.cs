using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.RoleDTOs;
using Clothy.UserService.BLL.DTOs.UserDTOs;
using Microsoft.AspNetCore.Identity;

namespace Clothy.UserService.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserReadDTO>> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<UserReadDTO> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserReadDTO> UpdateUserByIdAsync(UserUpdateDTO userUpdateDTO, ClaimsPrincipal currentUser, CancellationToken cancellationToken = default);
        Task<IdentityResult> DeleteUserByIdAsync(Guid userId, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default);
        Task<IdentityResult> AddUserToRoleAsync(Guid userId, AddUserRoleDTO roleDTO, CancellationToken cancellationToken = default);
        Task<IdentityResult> RemoveUserFromRoleAsync(Guid userId, RemoveUserRoleDTO roleDTO, CancellationToken cancellationToken = default);
    }
}
