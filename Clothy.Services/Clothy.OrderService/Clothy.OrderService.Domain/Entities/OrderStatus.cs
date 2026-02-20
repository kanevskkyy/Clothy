using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public enum OrderStatus
    {
        AwaitingPayment = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3
    }
}