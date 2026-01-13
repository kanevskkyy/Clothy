using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using Clothy.Shared.Events.EmailEvents.ClotheStockUpdated;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Consumers
{
    public class ClotheStockUpdatedConsumer : IConsumer<ClotheStockUpdatedEvent>
    {
        private IEmailService emailService;
        private ITemplateRender templateRender;
        private ILogger<ClotheStockUpdatedConsumer> logger;

        public ClotheStockUpdatedConsumer(IEmailService emailService, 
            ITemplateRender templateRender, 
            ILogger<ClotheStockUpdatedConsumer> logger)
        {
            this.emailService = emailService;
            this.templateRender = templateRender;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<ClotheStockUpdatedEvent> context)
        {
            logger.LogInformation("Received ClotheStockUpdatedEvent in NotificationService with ClotheId: {ClotheId}", context.Message.ClotheId);

            string html = await templateRender.RenderAsync("SendClotheAvaliable.cshtml", context.Message, context.CancellationToken);
            
            EmailMessageDTO emailMessageDTO = new EmailMessageDTO
            {
                To = context.Message.UserEmail,
                Subject = $"Clothy - Item Back in Stock",
                HtmlBody = html
            };
            await emailService.SendAsync(emailMessageDTO, context.CancellationToken);

            logger.LogInformation("Succesfully send email to email: {Email}", context.Message.UserEmail);
        }
    }
}
