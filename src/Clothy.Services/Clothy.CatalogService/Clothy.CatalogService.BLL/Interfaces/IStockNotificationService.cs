using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.Interfaces
{
    public interface IStockNotificationService
    {
        Task SubscribeForStockAsync(Guid stockId, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default);
        Task NotifySubscribersAsync(Guid stockId, CancellationToken cancellationToken = default);
    }
}
