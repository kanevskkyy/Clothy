using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Infrastructure.DB.Settings
{
    public class MongoDbSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public int MaxConnectionPoolSize { get; set; } = 100;
        public int MinConnectionPoolSize { get; set; } = 5;
        public int ConnectTimeoutSeconds { get; set; } = 10;
        public int SocketTimeoutSeconds { get; set; } = 10;
    }
}
