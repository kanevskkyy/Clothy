using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class DeliveryProvider : BaseEntity
    {
        public string? Name { get; set; }
        public string? IconUrl { get; set; }

        public ICollection<DeliveryDetail> DeliveryDetails { get; set; } = new List<DeliveryDetail>();
        public ICollection<PickupPoints> PickupPoints { get; set; } = new List<PickupPoints>();

    }
}
