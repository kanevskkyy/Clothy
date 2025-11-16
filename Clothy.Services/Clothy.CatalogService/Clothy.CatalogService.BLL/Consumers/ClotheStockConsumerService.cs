using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.EventLog;
using Clothy.Shared.Events;
using Clothy.Shared.Events.OrderEvents;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.BLL.Consumers
{
    public class ClotheStockConsumerService : IConsumer<OrderCreatedEvent>
    {
        private IClothesStockService clothesStockService;
        private ILogger<ClotheStockConsumerService> logger;
        private IEventLogService eventLogService;

        public ClotheStockConsumerService(IClothesStockService clothesStockService, ILogger<ClotheStockConsumerService> logger, IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService;
            this.clothesStockService = clothesStockService;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            logger.LogInformation("Received OrderCreatedEvent in CatalogService with OrderID: {OrderId}", context.Message.OrderId);

            Guid eventId = context.Message.EventId;
            if (await eventLogService.HasEventProcessedAsync(eventId, context.CancellationToken))
            {
                logger.LogWarning("Duplicate event detected: {EventId}", eventId);
                return;
            }

            foreach (OrderItemEvent orderItem in context.Message.Items)
            {
                await clothesStockService.UpdateStockAsync(orderItem.ClotheId, orderItem.ColorId, orderItem.SizeId, orderItem.Quantity, context.CancellationToken);
            }

            await eventLogService.MarkEventAsProcessedAsync(eventId, context.CancellationToken);

            logger.LogInformation("Successfully updated stock");
        }
    }
}
