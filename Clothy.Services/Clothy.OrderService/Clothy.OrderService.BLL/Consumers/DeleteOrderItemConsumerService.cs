using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.EventLog;
using Clothy.Shared.Events;
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
        private IEventLogService eventLogService;

        public DeleteOrderItemConsumerService(ILogger<DeleteOrderItemConsumerService> logger, IOrderItemService orderItemService, IEventLogService eventLogService)
        {
            this.logger = logger;
            this.eventLogService = eventLogService;
            this.orderItemService = orderItemService;
        }

        public async Task Consume(ConsumeContext<ClotheItemDeletedEvent> context)
        {
            Guid eventId = context.Message.EventId;
            if (await eventLogService.HasEventProcessedAsync(eventId))
            {
                logger.LogWarning("Duplicate ClotheItemDeletedEvent detected: {EventId}", eventId);
                return;
            }

            ClotheItemDeletedEvent clotheItemDeletedEvent = context.Message;
            logger.LogInformation("Received ClotheItemDeletedEvent for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);

            await orderItemService.SoftDeleteOrderItemsAsync(clotheItemDeletedEvent);

            await eventLogService.MarkEventAsProcessedAsync(eventId);

            logger.LogInformation("OrderItems are soft deleted for ClotheId: {ClotheId}", clotheItemDeletedEvent.ClotheId);
        }
    }
}
