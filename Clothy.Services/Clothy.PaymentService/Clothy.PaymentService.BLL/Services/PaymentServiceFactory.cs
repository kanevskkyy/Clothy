using Clothy.PaymentService.BLL.Services.Interfaces;
using Clothy.PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Services
{
    public class PaymentServiceFactory : IPaymentServiceFactory
    {
        private IEnumerable<IPaymentService> paymentServices;

        public PaymentServiceFactory(IEnumerable<IPaymentService> paymentServices)
        {
            this.paymentServices = paymentServices;
        }

        public IPaymentService GetPaymentService(PaymentMethod method)
        {
            IPaymentService? service = paymentServices.FirstOrDefault(s => s.PaymentMethod == method);

            if (service == null) throw new NotSupportedException($"Payment method {method} is not supported");
            return service;
        }
    }
}
