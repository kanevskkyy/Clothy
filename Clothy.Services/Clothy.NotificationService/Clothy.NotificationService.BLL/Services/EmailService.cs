using Clothy.NotificationService.BLL.DTOs;
using Clothy.NotificationService.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Services
{
    public class EmailService : IEmailService
    {
        private IEmailProvider emailProvider;

        public EmailService(IEmailProvider emailProvider)
        {
            this.emailProvider = emailProvider;
        }

        public async Task SendAsync(EmailMessageDTO emailMessageDTO, CancellationToken cancellationToken = default)
        {
            await emailProvider.SendEmailAsync(emailMessageDTO, cancellationToken);
        }
    }
}
