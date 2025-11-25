using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.Shared.Events.UserEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.Consumers
{
    public class UserUpdatedConsumer : IConsumer<UserUpdatedEvent>
    {
        private ILogger<UserUpdatedConsumer> logger;
        private IOrderService orderService;

        public UserUpdatedConsumer(ILogger<UserUpdatedConsumer> logger, IOrderService orderService)
        {
            this.logger = logger;
            this.orderService = orderService;
        }

        public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
        {
            UserUpdatedEvent userEvent = context.Message;
            logger.LogInformation("Received UserUpdatedEvent for UserId: {UserId}", userEvent.UserId);

            await orderService.HandleUserUpdatedEventAsync(userEvent);

            logger.LogInformation("Updated orders for UserId: {UserId}", userEvent.UserId);
        }
    }
}
