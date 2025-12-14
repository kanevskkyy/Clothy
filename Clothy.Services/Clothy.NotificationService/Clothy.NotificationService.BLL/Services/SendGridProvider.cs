using Clothy.NotificationService.BLL.Configuration;
using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Services
{
    public class SendGridProvider : IEmailProvider
    {
        private SendGridClient sendGridClient;
        private string fromEmail;
        private string fromName;


        public SendGridProvider(IOptions<EmailProviderOptions> options)
        {
            sendGridClient = new SendGridClient(options.Value.ApiKey);
            fromEmail = options?.Value?.FromEmail;
            fromName = options?.Value?.FromName;
        }
        
        public async Task SendEmailAsync(EmailMessageDTO emailMessageDTO, CancellationToken cancellationToken = default)
        {
            SendGridMessage message = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = emailMessageDTO.Subject,
                PlainTextContent = null,
                HtmlContent = emailMessageDTO.HtmlBody
            };

            message.AddTo(emailMessageDTO.To);
            message.SetClickTracking(false, false);


            await sendGridClient.SendEmailAsync(message, cancellationToken);
        }
    }
}
