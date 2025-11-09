using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.RegionCache
{
    public class RegionCacheInvalidationService : IEntityCacheInvalidationService<Region>
    {
        private IEntityCacheService cacheService;
        private ILogger<RegionCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "region:";
        private const string ALL_PATTERN = "region:*";

        public RegionCacheInvalidationService(IEntityCacheService cacheService, ILogger<RegionCacheInvalidationService> logger)
        {
            this.cacheService = cacheService;
            this.logger = logger;
        }

        public async Task InvalidateByIdAsync(Guid entityId)
        {
            try
            {
                string key = $"{CACHE_KEY_PREFIX}{entityId}";
                await cacheService.RemoveAsync(key);
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated cache for Region {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for Region {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated all Region caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all Region caches");
                throw;
            }
        }
    }
}
