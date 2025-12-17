using Clothy.Shared.Cache;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ServiceDefaults.Middleware.Redis
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                string redisHost = "clothy-redis";
                int redisPort = 6379;

                var config = ConfigurationOptions.Parse($"{redisHost}:{redisPort}");
                config.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(config);
            });

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024;
                options.CompactionPercentage = 0.2;
            });

            services.AddSingleton<IEntityCacheService, EntityCacheService>();

            return services;
        }
    }
}
