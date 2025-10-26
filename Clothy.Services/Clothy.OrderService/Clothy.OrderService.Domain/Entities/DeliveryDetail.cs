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

        public Guid ProviderId { get; set; }
        public DeliveryProvider? DeliveryProvider { get; set; }

        public Guid CityId { get; set; }
        public City? City { get; set; }

        public string? PostalIndex { get; set; }
        public string? PhoneNumber { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }

        public string? DetailsDescription { get; set; }
    }
}
