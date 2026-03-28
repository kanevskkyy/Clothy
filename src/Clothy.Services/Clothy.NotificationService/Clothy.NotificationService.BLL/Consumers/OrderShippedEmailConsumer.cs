using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using Clothy.Shared.Events.EmailEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Consumers
{
    public class OrderShippedEmailConsumer : IConsumer<OrderShippedEmailEvent>
    {
        private IEmailService emailService;
        private ITemplateRender templateRender;
        private ILogger<OrderShippedEmailConsumer> logger;

        public OrderShippedEmailConsumer(
            IEmailService emailService, 
            ITemplateRender templateRender, 
            ILogger<OrderShippedEmailConsumer> logger)
        {
            this.emailService = emailService;
            this.templateRender = templateRender;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderShippedEmailEvent> context)
        {
            logger.LogInformation("Received OrderShippedEmailEvent in NotificationService with OrderID: {OrderId}", context.Message.OrderId);

            string html = await templateRender.RenderAsync("SendOrderShipped.cshtml", "", cancellationToken: context.CancellationToken);

            EmailMessageDTO emailMessageDTO = new EmailMessageDTO()
            {
                To = context.Message.Email,
                Subject = $"Your order has been successfully shipped! OrderID: {context.Message.OrderId}",
                HtmlBody = html
            };
            await emailService.SendAsync(emailMessageDTO, context.CancellationToken);

            logger.LogInformation("Succesfully send email to email: {Email}", context.Message.Email);
        }
    }
}
