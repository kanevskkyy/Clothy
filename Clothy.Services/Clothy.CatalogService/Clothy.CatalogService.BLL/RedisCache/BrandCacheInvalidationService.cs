using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.RedisCache
{
    public class BrandCacheInvalidationService : IEntityCacheInvalidationService<Clothy.CatalogService.Domain.Entities.Catalog.Brand>
    {
        private IEntityCacheService cacheService;
        private ILogger<BrandCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "brand:";
        private const string LIST_PATTERN = "brands:*";
        private const string ALL_PATTERN = "brand:*";

        public BrandCacheInvalidationService(IEntityCacheService cacheService, ILogger<BrandCacheInvalidationService> logger)
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

                logger.LogInformation("Invalidated cache for Brand {EntityId} and all brand lists", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for Brand {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                await cacheService.RemoveByPatternAsync(LIST_PATTERN);

                logger.LogInformation("Invalidated all Brand-related caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all Brand caches");
                throw;
            }
        }
    }
}