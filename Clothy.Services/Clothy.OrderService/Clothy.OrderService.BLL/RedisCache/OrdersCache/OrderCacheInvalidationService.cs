using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.OrdersCache
{
    public class OrderCacheInvalidationService : IEntityCacheInvalidationService<Order>
    {
        private IEntityCacheService cacheService;
        private ILogger<OrderCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "order:";
        private const string LIST_PATTERN = "orders:pending:page:*";
        private const string ALL_PATTERN = "order:*";

        public OrderCacheInvalidationService(IEntityCacheService cacheService, ILogger<OrderCacheInvalidationService> logger)
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
                await cacheService.RemoveByPatternAsync(LIST_PATTERN);
                logger.LogInformation("Invalidated cache for Order {EntityId} and pending lists", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for Order {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                await cacheService.RemoveByPatternAsync(LIST_PATTERN);

                logger.LogInformation("Invalidated all Order-related caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all Order caches");
                throw;
            }
        }
    }
}
