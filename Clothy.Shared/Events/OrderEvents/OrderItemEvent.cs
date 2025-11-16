using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.OrderEvents
{
    public record OrderItemEvent
    {
        public Guid ClotheId { get; init; }
        public Guid ColorId { get; init; }
        public Guid SizeId { get; init; }
        public int Quantity { get; init; }
    }
}
