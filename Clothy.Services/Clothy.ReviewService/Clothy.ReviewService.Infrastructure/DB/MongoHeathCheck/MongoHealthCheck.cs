using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB.MongoHeathCheck
{
    public class MongoHealthCheck : IHealthCheck
    {
        private MongoDbContext dbContext;

        public MongoHealthCheck(MongoDbContext context)
        {
            dbContext = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                BsonDocumentCommand<BsonDocument> pingCommand = new BsonDocumentCommand<BsonDocument>(new BsonDocument("ping", 1));
                await dbContext.Database.RunCommandAsync(pingCommand, cancellationToken: cancellationToken);
                
                return HealthCheckResult.Healthy("MongoDB is healthy.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("MongoDB is unreachable.", ex);
            }
        }
    }
}
