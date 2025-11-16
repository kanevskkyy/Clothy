using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Infrastructure.DB;
using Clothy.Shared.Events;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.EventLog
{
    public class EventLogService : IEventLogService
    {
        private IMongoCollection<ProcessedEvent> collection;
        private IClientSessionHandle? session;

        public EventLogService(MongoDbContext context, IClientSessionHandle? session = null)
        {
            collection = context.ProcessedEvents;
            this.session = session;

            var indexKeys = Builders<ProcessedEvent>.IndexKeys.Ascending(x => x.EventId);
            CreateIndexOptions indexOptions = new CreateIndexOptions { Unique = true };
            collection.Indexes.CreateOne(new CreateIndexModel<ProcessedEvent>(indexKeys, indexOptions));
        }

        public async Task<bool> HasEventProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<ProcessedEvent>.Filter.Eq(x => x.EventId, eventId);
            long count;

            if (session != null) count = await collection.CountDocumentsAsync(session, filter, cancellationToken: cancellationToken);
            else count = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            return count > 0;
        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            ProcessedEvent processedEvent = new ProcessedEvent
            {
                EventId = eventId,
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                if (session != null) await collection.InsertOneAsync(session, processedEvent, cancellationToken: cancellationToken);
                else await collection.InsertOneAsync(processedEvent, cancellationToken: cancellationToken);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {

            }
        }
    }

}
