using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Config
{
    public class CryptoSettings : BasePaymentSettings
    {
        public string? CallbackURL { get; set; }
    }
}
