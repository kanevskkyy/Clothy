using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class StockNotificationRepository : GenericRepository<StockNotification>, IStockNotificationRepository
    {
        public StockNotificationRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<List<StockNotification>> GetAllSubscribersByStockId(Guid stockId, CancellationToken cancellationToken = default)
        {
            return await dbSet
                .Include(property => property.Stock)
                .ThenInclude(property => property!.Clothe)
                .Include(property => property.Stock)
                .ThenInclude(property => property!.Color)
                .Include(property => property.Stock)
                .ThenInclude(property => property!.Size)
                .Where(property => property.StockId == stockId && !property.IsNotified)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasUserAlreadySubscribeInStockId(Guid userId, CancellationToken cancellationToken = default)
        {
            return await dbSet.AnyAsync(property => property.UserId == userId && !property.IsNotified, cancellationToken);
        }
    }
}
