using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.SettlementCache
{
    public class SettlementCacheInvalidationService : IEntityCacheInvalidationService<Settlement>
    {
        private IEntityCacheService cacheService;
        private ILogger<SettlementCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "settlement:";
        private const string ALL_PATTERN = "settlement:*";

        public SettlementCacheInvalidationService(IEntityCacheService cacheService, ILogger<SettlementCacheInvalidationService> logger)
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
                logger.LogInformation("Invalidated cache for Settlement {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for Settlement {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated all Settlement caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all Settlement caches");
                throw;
            }
        }
    }
}
