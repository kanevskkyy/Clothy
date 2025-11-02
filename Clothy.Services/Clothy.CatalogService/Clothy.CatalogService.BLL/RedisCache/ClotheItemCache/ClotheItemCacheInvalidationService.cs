using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.BLL.RedisCache.Clothe
{
    public class ClotheItemCacheInvalidationService : IEntityCacheInvalidationService<ClotheItem>
    {
        private IEntityCacheService cacheService;
        private ILogger<ClotheItemCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "clothe:";
        private const string LIST_PATTERN = "clothes:page:*";
        private const string ALL_PATTERN = "clothe:*";

        public ClotheItemCacheInvalidationService(IEntityCacheService cacheService, ILogger<ClotheItemCacheInvalidationService> logger)
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

                logger.LogInformation("Invalidated cache for ClotheItem {EntityId} and paginated lists", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for ClotheItem {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                await cacheService.RemoveByPatternAsync(LIST_PATTERN);

                logger.LogInformation("Invalidated all ClotheItem-related caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all ClotheItem caches");
                throw;
            }
        }
    }
}