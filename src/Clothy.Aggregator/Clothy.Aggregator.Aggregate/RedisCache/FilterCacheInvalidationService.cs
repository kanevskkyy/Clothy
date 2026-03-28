using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.Aggregator.Aggregate.RedisCache
{
    public class FilterCacheInvalidationService : IFilterCacheInvalidationService
    {
        private IEntityCacheService cacheService;
        private ILogger<FilterCacheInvalidationService> logger;

        private const string FILTER_PATTERN = "filters:*";

        public FilterCacheInvalidationService(IEntityCacheService cacheService, ILogger<FilterCacheInvalidationService> logger)
        {
            this.cacheService = cacheService;
            this.logger = logger;
        }

        public async Task InvalidateAsync()
        {
            await cacheService.RemoveByPatternAsync(FILTER_PATTERN);
            logger.LogInformation("Invalidated all filter caches");
        }
    }
}
