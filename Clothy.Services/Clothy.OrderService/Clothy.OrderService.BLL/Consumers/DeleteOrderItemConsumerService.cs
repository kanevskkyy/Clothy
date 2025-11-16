using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.Shared.Events.ClotheItemEvents;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.Consumers
{
    public class DeleteOrderItemConsumerService : IConsumer<ClotheItemDeletedEvent>
    {
        private ILogger<DeleteOrderItemConsumerService> logger;
        private IOrderItemService orderItemService;

        public DeleteOrderItemConsumerService(ILogger<DeleteOrderItemConsumerService> logger, IOrderItemService orderItemService)
        {
            this.logger = logger;
            this.orderItemService = orderItemService;
        }

        public async Task Consume(ConsumeContext<ClotheItemDeletedEvent> context)
        {
            ClotheItemDeletedEvent clotheItemDeletedEvent = context.Message;
            logger.LogInformation("Received ClotheItemDeletedEvent for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);

            await orderItemService.SoftDeleteOrderItemsAsync(clotheItemDeletedEvent);

            logger.LogInformation("OrderItems are soft deleted for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);
        }
    }
}
