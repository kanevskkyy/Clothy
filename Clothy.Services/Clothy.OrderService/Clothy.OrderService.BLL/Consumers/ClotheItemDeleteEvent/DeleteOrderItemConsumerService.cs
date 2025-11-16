using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.Shared.Events.ClotheItem;
using Clothy.Shared.Events.ConsumerService;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.Consumers.ClotheItemDeleteEvent
{
    public class DeleteOrderItemConsumerService : IEventHandler<ClotheItemDeletedEvent>
    {
        private ILogger<DeleteOrderItemConsumerService> logger;
        private IOrderItemService orderItemService;

        public DeleteOrderItemConsumerService(ILogger<DeleteOrderItemConsumerService> logger, IOrderItemService orderItemService)
        {
            this.logger = logger;
            this.orderItemService = orderItemService;
        }

        public async Task HandleAsync(ClotheItemDeletedEvent clotheItemDeletedEvent, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Received ClotheItemDeleteEvent for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);

            await orderItemService.SoftDeleteOrderItemsAsync(clotheItemDeletedEvent, cancellationToken);

            logger.LogInformation("OrderItems are soft deleted for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);
        }
    }
}
