using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.BLL.RedisCache.ClotheItemCache
{
    public class ClotheItemCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private IClotheService clotheService;
        private ILogger<ClotheItemCachePreloader> logger;
        private static TimeSpan MEMORY_TTL = TimeSpan.FromMinutes(15);
        private static TimeSpan REDIS_TTL = TimeSpan.FromMinutes(45);
        
        private const int PAGE_SIZE = 10;
        private const int TOTAL_PAGES = 10;

        public ClotheItemCachePreloader(IEntityCacheService cacheService, IClotheService clotheService, ILogger<ClotheItemCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.clotheService = clotheService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting ClotheItemCachePreloader...");

            try
            {
                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    ClotheItemSpecificationParameters parameters = new ClotheItemSpecificationParameters
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE,
                    };

                    PagedList<ClotheSummaryDTO> pagedResult = await clotheService.GetPagedClotheItemsAsync(parameters, cancellationToken);

                    if (pagedResult == null)
                    {
                        logger.LogWarning("Paged clothe is null for page {Page}. Skipping cache.", page);
                        continue;
                    }

                    string cacheKey = $"clothes:page:{page}:size:{parameters.PageSize}";
                    await cacheService.SetAsync(cacheKey, pagedResult, MEMORY_TTL, REDIS_TTL);

                    logger.LogInformation("Preloaded ClotheItem page {Page} with {Count} items into cache with key {CacheKey}.", page, pagedResult.Items.Count, cacheKey);
                }

                List<(decimal min, decimal max)> rangeOfPrice = new List<(decimal min, decimal max)>
                {
                    (0, 100),
                    (100, 200),
                    (200, 500),
                    (500, 1000)
                };

                foreach ((decimal min, decimal max) in rangeOfPrice)
                {
                    for (int page = 1; page <= 3; page++)
                    {
                        ClotheItemSpecificationParameters parameters = new ClotheItemSpecificationParameters
                        {
                            PageNumber = page,
                            PageSize = 10,
                            MinPrice = min,
                            MaxPrice = max
                        };

                        PagedList<ClotheSummaryDTO> pagedResult = await clotheService.GetPagedClotheItemsAsync(parameters, cancellationToken);                        
                        string cacheKey = $"clothes:price:{min}-{max}:page:{page}:size:{parameters.PageSize}";
                        await cacheService.SetAsync(cacheKey, pagedResult, MEMORY_TTL, REDIS_TTL);

                        logger.LogInformation("Preloaded price range {Min}-{Max}, page {Page}, {Count} items", min, max, page, pagedResult.Items.Count);
                    }
                }


                logger.LogInformation("ClotheItemCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ClotheItemCachePreloader failed during cache warming.");
            }
        }
    }
}
