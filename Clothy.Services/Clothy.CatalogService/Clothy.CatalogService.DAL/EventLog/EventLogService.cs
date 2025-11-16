using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.EventLog
{
    public class EventLogService : IEventLogService
    {
        public ClothyCatalogDbContext context;

        public EventLogService(ClothyCatalogDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> HasEventProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await context.Set<ProcessedEvent>().AnyAsync(property => property.EventId == eventId, cancellationToken);

        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            context.Set<ProcessedEvent>().Add(new ProcessedEvent
            {
                EventId = eventId
            });
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
