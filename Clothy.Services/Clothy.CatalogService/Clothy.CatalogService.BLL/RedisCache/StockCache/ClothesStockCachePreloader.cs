using System;
using System.Threading;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.BLL.RedisCache.StockCache
{
    public class ClothesStockCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private IClothesStockService stockService;
        private ILogger<ClothesStockCachePreloader> logger;

        public ClothesStockCachePreloader(IEntityCacheService cacheService, IClothesStockService stockService, ILogger<ClothesStockCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.stockService = stockService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting ClothesStockCachePreloader...");

            try
            {
                const int PAGE_SIZE = 10;
                const int TOTAL_PAGES = 3;

                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    ClothesStockSpecificationParameters parameters = new ClothesStockSpecificationParameters
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE
                    };

                    PagedList<ClothesStockReadDTO> pagedStocks = await stockService.GetPagedClothesStockAsync(parameters, cancellationToken);

                    string cacheKey = $"clothesstock:page:{page}";
                    await cacheService.SetAsync(cacheKey, pagedStocks);

                    logger.LogInformation("Preloaded ClothesStock page {Page} with {Count} items into cache with key {CacheKey}.", page, pagedStocks.Items.Count, cacheKey);
                }

                logger.LogInformation("ClothesStockCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ClothesStockCachePreloader failed during cache warming.");
            }
        }
    }
}