using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.Shared.Events.OrderEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.BLL.Consumers
{
    public class ClotheStockConsumerService : IConsumer<OrderCreatedEvent>
    {
        private IClothesStockService clothesStockService;
        private ILogger<ClotheStockConsumerService> logger;

        public ClotheStockConsumerService(IClothesStockService clothesStockService, ILogger<ClotheStockConsumerService> logger)
        {
            this.clothesStockService = clothesStockService;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            logger.LogInformation("Received OrderCreatedEvent in CatalogService with OrderID: {OrderId}", context.Message.OrderId);

            foreach (OrderItemEvent orderItem in context.Message.Items)
            {
                await clothesStockService.UpdateStockAsync(orderItem.ClotheId, orderItem.ColorId, orderItem.SizeId, orderItem.Quantity, context.CancellationToken);
            }

            logger.LogInformation("Successfully updated stock");
        }
    }
}
