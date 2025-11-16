using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.ClotheItem
{
    public record ClotheItemUpdatedEvent : BaseEvent
    {
        public Guid ClotheId { get; init; }
        public string? ClotheName { get; init; }
        public decimal Price { get; init; }
        public string? MainPhoto { get; init; }
    }
}
