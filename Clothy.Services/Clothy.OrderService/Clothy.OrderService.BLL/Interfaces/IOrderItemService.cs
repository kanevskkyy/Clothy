using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events.ClotheItem;

namespace Clothy.OrderService.BLL.Interfaces
{
    public interface IOrderItemService
    {
        Task UpdateOrderItemsAsync(ClotheItemUpdatedEvent clotheItemUpdatedEvent, CancellationToken cancellationToken = default);
    }
}
