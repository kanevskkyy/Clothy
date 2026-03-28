using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.DAL.DB;
using Clothy.UserService.DAL.Repositories.Interfaces;
using Clothy.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clothy.UserService.DAL.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private UserDbContext dbContext;

        public RefreshTokenRepository(UserDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return refreshToken;
        }

        public async Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await dbContext.RefreshTokens.FirstOrDefaultAsync(property => property.Token == refreshToken, cancellationToken);
        }

        public async Task<RefreshToken?> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            dbContext.RefreshTokens.Update(refreshToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return refreshToken;
        }
    }
}
