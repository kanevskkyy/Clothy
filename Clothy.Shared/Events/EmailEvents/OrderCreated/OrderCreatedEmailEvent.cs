using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.EmailEvents.OrderCreated
{
    public record OrderCreatedEmailEvent : BaseEvent
    {
        public Guid OrderId { get; init; }
        public string? UserEmail { get; init; }
        public decimal TotalPrice { get; init; }
        public List<OrderItemEmailEvent> Items { get; set; } = new List<OrderItemEmailEvent>();
    }
}
