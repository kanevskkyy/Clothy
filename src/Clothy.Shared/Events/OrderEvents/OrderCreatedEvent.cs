using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events;

namespace Clothy.Shared.Events.OrderEvents
{
    public record OrderCreatedEvent : BaseEvent
    {
        public Guid OrderId { get; init; }
        public List<OrderItemEvent> Items { get; init; } = new();
    }
}
