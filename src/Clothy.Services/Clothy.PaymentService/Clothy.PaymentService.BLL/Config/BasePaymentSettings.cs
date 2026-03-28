using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Config
{
    public abstract class BasePaymentSettings
    {
        public string? ApiKey { get; set; }
        public string? WebhookSecret { get; set; }
        public string? SuccessURL { get; set; }
        public string? CancelURL { get; set; }
    }
}
