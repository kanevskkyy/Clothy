using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.EmailEvents
{
    public record OrderDeliveredEmailEvent : BaseEvent
    {
        public Guid OrderId { get; init; }
        public string? Email { get; init; }
    }
}
