using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events;

namespace Clothy.Shared.Events.ClotheItemEvents
{
    public record ClotheItemUpdatedEvent : BaseEvent
    {
        public Guid ClotheId { get; init; }
        public string? ClotheName { get; init; }
        public decimal Price { get; init; }
        public string? MainPhoto { get; init; }
    }
}
