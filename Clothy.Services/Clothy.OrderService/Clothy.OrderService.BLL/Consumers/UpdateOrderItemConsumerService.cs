using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
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

        public UpdateOrderItemConsumerService(ILogger<UpdateOrderItemConsumerService> logger, IOrderItemService orderItemService)
        {
            this.logger = logger;
            this.orderItemService = orderItemService;
        }

        public async Task Consume(ConsumeContext<ClotheItemUpdatedEvent> context)
        {
            ClotheItemUpdatedEvent clotheItemEvent = context.Message;
            logger.LogInformation("Received ClotheItemUpdatedEvent for ClotheId: {ClotheId}", clotheItemEvent.ClotheId);

            await orderItemService.UpdateOrderItemsAsync(clotheItemEvent);

            logger.LogInformation("Orders updated for ClotheId: {ClotheId}", clotheItemEvent.ClotheId);
        }
    }
}
