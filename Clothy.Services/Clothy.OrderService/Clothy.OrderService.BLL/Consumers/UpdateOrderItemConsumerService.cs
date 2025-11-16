using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.EventLog;
using Clothy.OrderService.DAL.UOW;
using Clothy.Shared.Events;
using Clothy.Shared.Events.ClotheItemEvents;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.Consumers
{
    public class UpdateOrderItemConsumerService : IConsumer<ClotheItemUpdatedEvent>
    {
        private ILogger<UpdateOrderItemConsumerService> logger;
        private IOrderItemService orderItemService;
        private IEventLogService eventLogService;

        public UpdateOrderItemConsumerService(ILogger<UpdateOrderItemConsumerService> logger, IOrderItemService orderItemService, IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService;
            this.logger = logger;
            this.orderItemService = orderItemService;
        }

        public async Task Consume(ConsumeContext<ClotheItemUpdatedEvent> context)
        {
            Guid eventId = context.Message.EventId;

            if (await eventLogService.HasEventProcessedAsync(eventId))
            {
                logger.LogWarning("Duplicate ClotheItemUpdatedEvent detected: {EventId}", eventId);
                return;
            }

            ClotheItemUpdatedEvent clotheItemEvent = context.Message;
            logger.LogInformation("Received ClotheItemUpdatedEvent for ClotheId: {ClotheId}", clotheItemEvent.ClotheId);

            await orderItemService.UpdateOrderItemsAsync(clotheItemEvent);
            await eventLogService.MarkEventAsProcessedAsync(eventId);

            logger.LogInformation("Orders updated for ClotheId: {ClotheId}", clotheItemEvent.ClotheId);
        }
    }
}
