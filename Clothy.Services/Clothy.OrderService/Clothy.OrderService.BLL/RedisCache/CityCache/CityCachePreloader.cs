using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.CityCache
{
    public class CityCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private ICityService cityService;
        private ILogger<CityCachePreloader> logger;

        private const int PAGE_SIZE = 10;
        private const int TOTAL_PAGES = 3;

        public CityCachePreloader(IEntityCacheService cacheService, ICityService cityService, ILogger<CityCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.cityService = cityService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting CityCachePreloader...");

            try
            {
                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    CityFilterDTO filter = new CityFilterDTO
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE
                    };

                    PagedList<CityReadDTO> pagedResult = await cityService.GetPagedAsync(filter, cancellationToken);

                    foreach (CityReadDTO city in pagedResult.Items)
                    {
                        string cacheKey = $"city:{city.Id}";
                        await cacheService.SetAsync(cacheKey, city);
                        logger.LogInformation("Preloaded City {Name} ({Id}) into cache with key {CacheKey}", city.Name, city.Id, cacheKey);
                    }

                    if (pagedResult.Items.Count < PAGE_SIZE) break; 
                }

                logger.LogInformation("CityCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CityCachePreloader failed during cache warming.");
            }
        }
    }
}
