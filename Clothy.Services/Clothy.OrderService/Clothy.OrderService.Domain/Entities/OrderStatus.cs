using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class OrderStatus : BaseEntity
    {
        public string? Name { get; set; }
        public string? IconUrl { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
