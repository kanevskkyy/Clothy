using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.CityCache
{
    public class CityCacheInvalidationService : IEntityCacheInvalidationService<City>
    {
        private IEntityCacheService cacheService;
        private ILogger<CityCacheInvalidationService> logger;

        private const string CACHE_KEY_PREFIX = "city:";
        private const string ALL_PATTERN = "city:*";

        public CityCacheInvalidationService(IEntityCacheService cacheService, ILogger<CityCacheInvalidationService> logger)
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
                logger.LogInformation("Invalidated cache for City {EntityId}", entityId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate cache for City {EntityId}", entityId);
                throw;
            }
        }

        public async Task InvalidateAllAsync()
        {
            try
            {
                await cacheService.RemoveByPatternAsync(ALL_PATTERN);
                logger.LogInformation("Invalidated all City caches");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate all City caches");
                throw;
            }
        }
    }
}
