using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Events;

namespace Clothy.Shared.Events.ClotheItemEvents
{
    public record ClotheItemDeletedEvent : BaseEvent
    {
        public Guid ClotheId { get; init; }
    }
}
