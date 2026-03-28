using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events
{
    public record BaseEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public string? CorrelationId { get; init; }
        public DateTime EventHappend { get; init; } = DateTime.UtcNow;
    }
}
