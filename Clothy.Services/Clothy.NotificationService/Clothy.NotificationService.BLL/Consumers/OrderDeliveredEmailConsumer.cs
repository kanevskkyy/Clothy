using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using Clothy.Shared.Events.EmailEvents.OrderDelivered;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Consumers
{
    public class OrderDeliveredEmailConsumer : IConsumer<OrderDeliveredEmailEvent>
    {
        private IEmailService emailService;
        private ITemplateRender templateRender;
        private ILogger<OrderDeliveredEmailConsumer> logger;

        public OrderDeliveredEmailConsumer(
            IEmailService emailService, 
            ITemplateRender templateRender, 
            ILogger<OrderDeliveredEmailConsumer> logger)
        {
            this.emailService = emailService;
            this.templateRender = templateRender;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDeliveredEmailEvent> context)
        {
            logger.LogInformation("Received OrderDeliveredEmailEvent in NotificationService with OrderID: {OrderId}", context.Message.OrderId);

            string html = await templateRender.RenderAsync("SendOrderDelivered.cshtml", "", cancellationToken: context.CancellationToken);

            EmailMessageDTO emailMessageDTO = new EmailMessageDTO()
            {
                To = context.Message.Email,
                Subject = $"Your order has been successfully delivered! OrderID: {context.Message.OrderId}",
                HtmlBody = html
            };
            await emailService.SendAsync(emailMessageDTO, context.CancellationToken);

            logger.LogInformation("Succesfully send email to email: {Email}", context.Message.Email);
        }
    }
}