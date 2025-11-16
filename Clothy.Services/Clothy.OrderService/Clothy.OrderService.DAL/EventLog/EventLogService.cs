using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.ConnectionFactory;
using Clothy.Shared.Events;
using Dapper;

namespace Clothy.OrderService.DAL.EventLog
{
    public class EventLogService : IEventLogService
    {
        private IConnectionFactory connectionFactory;

        public EventLogService(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<bool> HasEventProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = connectionFactory.CreateConnection();

            const string sql = "SELECT COUNT(1) FROM processed_events WHERE eventid = @EventId";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { EventId = eventId });
            return count > 0;
        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            using IDbConnection connection = connectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO processed_events(eventid, processedat)
                VALUES(@EventId, NOW() AT TIME ZONE 'utc')
                ON CONFLICT(eventid) DO NOTHING";

            await connection.ExecuteAsync(sql, new { EventId = eventId });
        }
    }
}
