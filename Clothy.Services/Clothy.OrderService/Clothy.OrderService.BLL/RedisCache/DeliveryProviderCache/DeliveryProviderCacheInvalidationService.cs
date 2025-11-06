using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.DeliveryProviderCache
{
    public class DeliveryProviderCacheInvalidationService : IEntityCacheInvalidationService<DeliveryProvider>
    {
        private IEntityCacheService cacheService;
        private ILogger<DeliveryProviderCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "delivery-provider:";
        private const string ALL_PATTERN = "delivery-provider:*";

        public DeliveryProviderCacheInvalidationService(IEntityCacheService cacheService, ILogger<DeliveryProviderCacheInvalidationService> logger)
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
                logger.LogInformation("Invalidated cache for DeliveryProvider {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for DeliveryProvider {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated all DeliveryProvider caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all DeliveryProvider caches");
                throw;
            }
        }
    }
}
