using Clothy.NotificationService.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessageDTO emailMessageDTO, CancellationToken cancellationToken = default);
    }
}
