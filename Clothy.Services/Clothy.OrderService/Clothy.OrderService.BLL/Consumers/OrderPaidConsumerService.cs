using Clothy.OrderService.BLL.Interfaces;
using Clothy.Shared.Events;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.Shared.Events.PaymentEvents;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.Consumers
{
    public class OrderPaidConsumerService : IConsumer<OrderPaidEvent>
    {
        private IEventLogService eventLogService;
        private IOrderService orderService;
        private ILogger<OrderPaidConsumerService> logger;

        public OrderPaidConsumerService(
            IEventLogService eventLogService, 
            IOrderService orderService, 
            ILogger<OrderPaidConsumerService> logger)
        {
            this.eventLogService = eventLogService;
            this.orderService = orderService;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPaidEvent> context)
        {
            Guid eventId = context.Message.EventId;
            
            if (await eventLogService.HasEventProcessedAsync(eventId))
            {
                logger.LogWarning("Duplicate OrderPaidEvent detected: {EventId}", eventId);
                return;
            }

            OrderPaidEvent orderPaidEvent = context.Message;
            logger.LogInformation("Received OrderPaidEvent for OrderId: {OrderId}", orderPaidEvent.OrderId);

            await orderService.HandleOrderPaidEventAsync(orderPaidEvent, context.CancellationToken);
            await eventLogService.MarkEventAsProcessedAsync(eventId, context.CancellationToken);

            logger.LogInformation("Order with ID: {OrderId} marked as paid", orderPaidEvent.OrderId);
        }
    }
}
