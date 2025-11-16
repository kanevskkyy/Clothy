using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Events
{
    public interface IEventLogService
    {
        Task<bool> HasEventProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
