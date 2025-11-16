using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events.ClotheItemEvents;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IOrderItemService
    {
        Task SoftDeleteOrderItemsAsync(ClotheItemDeletedEvent clotheItemDeletedEvent, CancellationToken cancellationToken = default);
        Task UpdateOrderItemsAsync(ClotheItemUpdatedEvent clotheItemUpdatedEvent, CancellationToken cancellationToken = default);
    }
}
