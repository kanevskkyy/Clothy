using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.UserEvents
{
    public record UserDeletedEvent : BaseEvent
    {
        public Guid UserId { get; init; }
    }
}
