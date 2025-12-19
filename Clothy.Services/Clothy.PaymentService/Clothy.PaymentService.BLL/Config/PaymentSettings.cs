using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Config
{
    public class PaymentSettings
    {
        public string? SecretKey { get; set; } 
        public string? PublishableKey { get; set; } 
        public string? SuccessUrl { get; set; } 
        public string? CancelUrl { get; set; }
        public string? WebhookSecret { get; set; } 
    }
}
