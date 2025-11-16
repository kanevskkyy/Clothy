using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.Shared.Events.ClotheItem;
using Clothy.Shared.Events.ConsumerService;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.Consumers.ClotheItemUpdateEvent
{
    public class UpdateOrderItemConsumerService : IEventHandler<ClotheItemUpdatedEvent>
    {
        private ILogger<UpdateOrderItemConsumerService> logger;
        private IOrderItemService orderItemService;

        public UpdateOrderItemConsumerService(ILogger<UpdateOrderItemConsumerService> logger, IOrderItemService orderItemService)
        {
            this.logger = logger;
            this.orderItemService = orderItemService;
        }

        public async Task HandleAsync(ClotheItemUpdatedEvent clotheItemUpdatedEvent)
        {
            logger.LogInformation("Received ClotheItemUpdatedEvent for ClotheId: {ClotheId}", clotheItemUpdatedEvent.ClotheId);

            await orderItemService.UpdateOrderItemsAsync(clotheItemUpdatedEvent);
            
            logger.LogInformation("Orders updated for ClotheId: {ClotheId}", clotheItemUpdatedEvent.ClotheId);
        }
    }
}
