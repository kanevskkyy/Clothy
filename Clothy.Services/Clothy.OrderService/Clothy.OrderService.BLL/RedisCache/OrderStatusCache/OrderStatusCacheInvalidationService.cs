using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.OrderStatusCache
{
    public class OrderStatusCacheInvalidationService : IEntityCacheInvalidationService<OrderStatus>
    {
        private IEntityCacheService cacheService;
        private ILogger<OrderStatusCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "order-status:";
        private const string ALL_PATTERN = "order-status:*";

        public OrderStatusCacheInvalidationService(IEntityCacheService cacheService, ILogger<OrderStatusCacheInvalidationService> logger)
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

                logger.LogInformation("Invalidated cache for OrderStatus {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for OrderStatus {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated all OrderStatus caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all OrderStatus caches");
                throw;
            }
        }
    }

}
