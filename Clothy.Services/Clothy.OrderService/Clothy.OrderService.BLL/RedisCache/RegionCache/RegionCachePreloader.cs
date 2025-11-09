using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.RegionCache
{
    public class RegionCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private IRegionService regionService;
        private ILogger<RegionCachePreloader> logger;

        private static TimeSpan MEMORY_TTL = TimeSpan.FromHours(6);
        private static TimeSpan REDIS_TTL = TimeSpan.FromDays(1);

        private const int PAGE_SIZE = 10;
        private const int TOTAL_PAGES = 3;

        public RegionCachePreloader(IEntityCacheService cacheService, IRegionService regionService, ILogger<RegionCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.regionService = regionService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting RegionCachePreloader...");

            try
            {
                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    RegionFilterDTO filter = new RegionFilterDTO
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE
                    };

                    PagedList<RegionReadDTO> pagedResult = await regionService.GetPagedAsync(filter, cancellationToken);
                    if (pagedResult == null)
                    {
                        logger.LogWarning("Paged regions is null for page {Page}. Skipping cache.", page);
                        continue;
                    }

                    string cacheKey = $"regions:page:{page}:size:{PAGE_SIZE}";
                    await cacheService.SetAsync(cacheKey, pagedResult, MEMORY_TTL, REDIS_TTL);

                    logger.LogInformation("Preloaded Regions page {Page} with {Count} items into cache with key {CacheKey}.", page, pagedResult.Items.Count, cacheKey);

                    if (pagedResult.Items.Count < PAGE_SIZE) break;
                }

                logger.LogInformation("RegionCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RegionCachePreloader failed during cache warming.");
            }
        }
    }
}
