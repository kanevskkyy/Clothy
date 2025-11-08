using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities.AdditionalEntities
{
    public class DeliveryDetailData
    {
        public Guid Id { get; set; }
        public DeliveryProvider? DeliveryProvider { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
