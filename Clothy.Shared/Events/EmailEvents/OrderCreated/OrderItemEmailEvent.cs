using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.EmailEvents.OrderCreated
{
    public record OrderItemEmailEvent
    {
        public string? ClotheName { get; init; }
        public string? Size { get; init; }
        public string? Color { get; init; }
        public int Quantity { get; init; }
        public decimal Price { get; init; }
    }
}
