using Clothy.PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Services.Interfaces
{
    public interface IPaymentServiceFactory
    {
        IPaymentService GetPaymentService(PaymentMethod method);
    }
}
