using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid StatusId { get; set; }
        public OrderStatus? Status { get; set; }

        public Guid UserId { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public DeliveryDetail? DeliveryDetail { get; set; }
    }
}
