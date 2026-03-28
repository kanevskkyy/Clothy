using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.Domain.Entities;

namespace Clothy.UserService.DAL.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task<RefreshToken?> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    }
}
