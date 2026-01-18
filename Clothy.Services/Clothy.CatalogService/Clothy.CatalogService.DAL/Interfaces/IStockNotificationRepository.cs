using Clothy.CatalogService.Domain.Entities.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.DAL.Interfaces
{
    public interface IStockNotificationRepository : IGenericRepository<StockNotification>
    {
        Task<bool> HasUserAlreadySubscribeInStockId(Guid userId, CancellationToken cancellationToken = default);
        Task<List<StockNotification>> GetAllSubscribersByStockId(Guid stockId, CancellationToken cancellationToken = default);
    }
}
