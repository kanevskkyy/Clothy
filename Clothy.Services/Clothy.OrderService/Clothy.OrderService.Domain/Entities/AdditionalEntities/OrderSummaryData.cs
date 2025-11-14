using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities.AdditionalEntities
{
    public class OrderSummaryData
    {
        public Guid Id { get; set; }
        public OrderStatus? Status { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
