using Clothy.PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.DTOs
{
    public class CreatePaymentRequestDTO
    {
        public Guid OrderId { get; set; }
    }
}
