using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.PickupPointsCache
{
    public class PickupPointCachePreloader : ICachePreloader
    {
        private readonly IEntityCacheService cacheService;
        private readonly IPickupPointService pickupPointService;
        private readonly ILogger<PickupPointCachePreloader> logger;

        private static TimeSpan MEMORY_TTL = TimeSpan.FromHours(6);
        private static TimeSpan REDIS_TTL = TimeSpan.FromDays(1);

        private const int PAGE_SIZE = 10;
        private const int TOTAL_PAGES = 3;

        public PickupPointCachePreloader(IEntityCacheService cacheService, IPickupPointService pickupPointService, ILogger<PickupPointCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.pickupPointService = pickupPointService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting PickupPointCachePreloader...");

            try
            {
                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    PickupPointFilterDTO filter = new PickupPointFilterDTO
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE
                    };

                    PagedList<PickupPointReadDTO> pagedResult = await pickupPointService.GetPagedAsync(filter, cancellationToken);
                    if (pagedResult == null)
                    {
                        logger.LogWarning("Paged pickup points is null for page {Page}. Skipping cache.", page);
                        continue;
                    }

                    string cacheKey = $"pickuppoint:page:{page}:size:{PAGE_SIZE}";
                    await cacheService.SetAsync(cacheKey, pagedResult, MEMORY_TTL, REDIS_TTL);

                    logger.LogInformation("Preloaded PickupPoints page {Page} with {Count} items into cache with key {CacheKey}.", page, pagedResult.Items.Count, cacheKey);

                    if (pagedResult.Items.Count < PAGE_SIZE) break;
                }

                logger.LogInformation("PickupPointCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PickupPointCachePreloader failed during cache warming.");
            }
        }
    }
}
