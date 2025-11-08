using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class DeliveryDetail : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public string? PhoneNumber { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }

    }
}
