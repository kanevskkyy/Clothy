using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.BLL.RedisCache.StockCache
{
    public class ClothesStockCacheInvalidationService : IEntityCacheInvalidationService<ClothesStock>
    {
        private IEntityCacheService cacheService;
        private ILogger<ClothesStockCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "clothesstock:";
        private const string TOP_STOCK_PATTERN = "clothes:topstock:*";
        private const string ALL_PATTERN = "clothesstock:*";

        public ClothesStockCacheInvalidationService(IEntityCacheService cacheService, ILogger<ClothesStockCacheInvalidationService> logger)
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
                await cacheService.RemoveByPatternAsync(TOP_STOCK_PATTERN);

                logger.LogInformation("Invalidated cache for ClothesStock {EntityId} and top stock list", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for ClothesStock {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                await cacheService.RemoveByPatternAsync(TOP_STOCK_PATTERN);

                logger.LogInformation("Invalidated all ClothesStock-related caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all ClothesStock caches");
                throw;
            }
        }
    }
}
