using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class City : BaseEntity
    {
        public string? Name { get; set; }

        public ICollection<DeliveryDetail> DeliveryDetails { get; set; } = new List<DeliveryDetail>();
        public ICollection<Region> Regions { get; set; } = new List<Region>();
    }
}
