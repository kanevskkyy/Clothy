using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Infrastructure.Settings;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB
{
    public class MongoDbContext
    {
        private IMongoDatabase database;
        public IMongoClient Client { get; }

        public MongoDbContext(MongoDbSettings settings)
        {
            MongoClientSettings mongoClientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            mongoClientSettings.MaxConnectionPoolSize = settings.MaxConnectionPoolSize;
            mongoClientSettings.MinConnectionPoolSize = settings.MinConnectionPoolSize;
            mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(settings.ConnectTimeoutSeconds);
            mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(settings.SocketTimeoutSeconds);

            Client = new MongoClient(mongoClientSettings);
            database = Client.GetDatabase(settings.DatabaseName);
        }

        public IMongoDatabase Database => database;
    }
}
