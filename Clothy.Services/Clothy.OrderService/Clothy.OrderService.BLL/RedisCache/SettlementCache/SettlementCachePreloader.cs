using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.SettlementCache
{
    public class SettlementCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private ISettlementService settlementService;
        private ILogger<SettlementCachePreloader> logger;

        private static readonly TimeSpan MEMORY_TTL = TimeSpan.FromHours(6);
        private static readonly TimeSpan REDIS_TTL = TimeSpan.FromDays(1);

        private const int PAGE_SIZE = 10;
        private const int TOTAL_PAGES = 3;

        public SettlementCachePreloader(IEntityCacheService cacheService, ISettlementService settlementService, ILogger<SettlementCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.settlementService = settlementService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting SettlementCachePreloader...");

            try
            {
                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    SettlementFilterDTO filter = new SettlementFilterDTO
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE
                    };

                    PagedList<SettlementReadDTO> pagedResult = await settlementService.GetPagedAsync(filter, cancellationToken);
                    if (pagedResult == null)
                    {
                        logger.LogWarning("Paged settlements is null for page {Page}. Skipping cache.", page);
                        continue;
                    }

                    string cacheKey = $"settlements:page:{page}:size:{PAGE_SIZE}";
                    await cacheService.SetAsync(cacheKey, pagedResult, MEMORY_TTL, REDIS_TTL);

                    logger.LogInformation("Preloaded Settlements page {Page} with {Count} items into cache with key {CacheKey}.", page, pagedResult.Items.Count, cacheKey);

                    if (pagedResult.Items.Count < PAGE_SIZE) break;
                }

                logger.LogInformation("SettlementCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SettlementCachePreloader failed during cache warming.");
            }
        }
    }
}
