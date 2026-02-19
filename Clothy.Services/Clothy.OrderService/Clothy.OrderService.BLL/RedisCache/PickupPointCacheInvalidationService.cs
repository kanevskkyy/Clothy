using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.PickupPointsCache
{
    public class PickupPointCacheInvalidationService : IEntityCacheInvalidationService<PickupPoints>
    {
        private IEntityCacheService cacheService;
        private ILogger<PickupPointCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "pickuppoint:";
        private const string ALL_PATTERN = "pickuppoint:*";

        public PickupPointCacheInvalidationService(IEntityCacheService cacheService, ILogger<PickupPointCacheInvalidationService> logger)
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
                logger.LogInformation("Invalidated cache for PickupPoint {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for PickupPoint {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated all PickupPoint caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all PickupPoint caches");
                throw;
            }
        }
    }
}
