using Clothy.AuthService.BLL.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.Services.Interfaces
{
    public interface IKeycloakUserHelper
    {
        Task<UserReadDTO> GetUserByEmailAsync(string email, string adminToken, CancellationToken cancellationToken = default);
        Task<UserReadDTO> GetUserByIdAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
        Task<string> GetUserIdByEmailAsync(string email, string adminToken, CancellationToken cancellationToken = default);
    }
}
