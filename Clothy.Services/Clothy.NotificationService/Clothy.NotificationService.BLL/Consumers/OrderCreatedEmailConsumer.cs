using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using Clothy.Shared.Events.EmailEvents.OrderCreated;
using MassTransit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using RazorLight.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Consumers
{
    public class OrderCreatedEmailConsumer : IConsumer<OrderCreatedEmailEvent>
    {
        private IEmailService emailService;
        private ITemplateRender templateRender;
        private ILogger<OrderCreatedEmailConsumer> logger;

        public OrderCreatedEmailConsumer(
            IEmailService emailService, 
            ITemplateRender templateRender, 
            ILogger<OrderCreatedEmailConsumer> logger)
        {
            this.logger = logger;
            this.emailService = emailService;
            this.templateRender = templateRender;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEmailEvent> context)
        {
            logger.LogInformation("Received OrderCreatedEmailEvent in NotificationService with OrderID: {OrderId}", context.Message.OrderId);

            string html = await templateRender.RenderAsync("SendCheckout.cshtml", context.Message, context.CancellationToken);

            EmailMessageDTO emailMessageDTO = new EmailMessageDTO() 
            {
                To = context.Message.UserEmail,
                Subject = $"Your order has been successfully placed! OrderID: {context.Message.OrderId}",
                HtmlBody = html
            };
            await emailService.SendAsync(emailMessageDTO, context.CancellationToken);

            logger.LogInformation("Succesfully send email to email: {Email}", context.Message.UserEmail);
        }
    }
}
