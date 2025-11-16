using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events.ClotheItem;
using Clothy.Shared.Events.ConsumerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Clothy.OrderService.BLL.Consumers.ClotheItemUpdateEvent
{
    public class OrderItemUpdatedListenerService : BaseEventListenerService<ClotheItemUpdatedEvent>
    {
        protected override string ExchangeName => "clothe-item-updated";
        protected override string QueueName => "order-service-clothe-item-updated-queue";
        protected override string RoutingKey => "clothe-item-updated-key";

        public OrderItemUpdatedListenerService(IConnectionFactory connectionFactory, ILogger<OrderItemUpdatedListenerService> logger, IServiceScopeFactory serviceScopeFactory): base(connectionFactory, logger, serviceScopeFactory)
        {

        }
    }
}
