using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.EmailEvents.ClotheStockUpdated
{
    public record ClotheStockUpdatedEvent : BaseEvent
    {
        public string? UserFirstName { get; init; }
        public string? UserEmail { get; init; }
        public Guid ClotheId { get; init; }
        public string? ClotheName { get; init; }
        public string? Size { get; init; }
        public string? Color { get; init; }
    }
}
