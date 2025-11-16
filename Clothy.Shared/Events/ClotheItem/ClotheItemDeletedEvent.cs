using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.ClotheItem
{
    public record ClotheItemDeletedEvent : BaseEvent
    {
        public Guid ClotheId { get; init; }
    }
}
