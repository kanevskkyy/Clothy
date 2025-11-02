using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Interfaces;
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
                for (int page = 1; page <= 10; page++)
                {
                    ClotheItemSpecificationParameters parameters = new ClotheItemSpecificationParameters
                    {
                        PageNumber = page,
                        PageSize = 10,
                    };

                    PagedList<ClotheSummaryDTO> pagedResult = await clotheService.GetPagedClotheItemsAsync(parameters, cancellationToken);
                    string cacheKey = $"clothes:page:{page}:size:{parameters.PageSize}";
                    await cacheService.SetAsync(cacheKey, pagedResult);

                    logger.LogInformation("Preloaded ClotheItem page {Page} with {Count} items into cache with key {CacheKey}.", page, pagedResult.Items.Count, cacheKey);
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
