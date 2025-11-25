using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events.UserEvents
{
    public record UserUpdatedEvent : BaseEvent
    {
        public Guid UserId { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}
