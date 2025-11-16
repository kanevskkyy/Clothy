using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events.ClotheItem;
using Clothy.Shared.Events.ConsumerService;
using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Clothy.OrderService.BLL.Consumers.ClotheItemDeleteEvent
{
    public class OrderItemDeletedListenerService : BaseEventListenerService<ClotheItemDeletedEvent>
    {
        protected override string ExchangeName => "clothe-item-deleted";
        protected override string QueueName => "delete-clothe-item-queue";
        protected override string RoutingKey => "clothe-item-deleted-orders-key";

        public OrderItemDeletedListenerService(IConnectionFactory connectionFactory, ILogger<OrderItemDeletedListenerService> logger, IServiceScopeFactory serviceScopeFactory) : base(connectionFactory, logger, serviceScopeFactory)
        {

        }
    }
}
