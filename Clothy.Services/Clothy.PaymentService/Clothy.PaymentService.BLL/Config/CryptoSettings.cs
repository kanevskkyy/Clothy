using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Config
{
    public class CryptoSettings : BasePaymentSettings
    {
        public string BaseURL { get; set; } = "https://api.nowpayments.io/v1/invoice/";
        public string PriceCurrency { get; set; } = "usd";
        public string PayCurrency { get; set; } = "usdc";
        public string ApiKeyHeader { get; set; } = "x-api-key";
        public string? CallbackURL { get; set; }
    }
}
