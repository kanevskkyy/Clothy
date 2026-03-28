using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.BasketService.BLL.Services.Interfaces;
using Clothy.Shared.Events.UserEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Clothy.BasketService.BLL.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private ILogger<UserDeletedConsumer> logger;
        private IBasketService basketService;

        public UserDeletedConsumer(ILogger<UserDeletedConsumer> logger, IBasketService basketService)
        {
            this.logger = logger;
            this.basketService = basketService;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            Guid eventId = context.Message.EventId;
            UserDeletedEvent userDeletedEvent = context.Message;

            logger.LogInformation("BasketService: Received UserDeletedEvent for UserID: {UserId}", userDeletedEvent.UserId);

            await basketService.ClearBasketAsync(userDeletedEvent.UserId);

            logger.LogInformation("BasketService: Clear all basket for UserId: {UserId}", userDeletedEvent.UserId);
        }
    }
}
