using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities.AdditionalEntities
{
    public class OrderWithDetailsData
    {
        public Guid Id { get; set; }
        public OrderStatus? Status { get; set; } 
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemData> Items { get; set; } = new List<OrderItemData>();
        public DeliveryDetailData? DeliveryDetail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
