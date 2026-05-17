using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.NotificationService.BLL.Configuration
{
    public class EmailProviderOptions
    {
        public string? ApiKey { get; set; } 
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
    }
}
