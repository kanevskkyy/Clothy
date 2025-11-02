using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.DeliveryProviderCache
{
    public class DeliveryProviderCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private IDeliveryProviderService providerService;
        private ILogger<DeliveryProviderCachePreloader> logger;

        public DeliveryProviderCachePreloader(IEntityCacheService cacheService, IDeliveryProviderService providerService, ILogger<DeliveryProviderCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.providerService = providerService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting DeliveryProviderCachePreloader...");

            try
            {
                List<DeliveryProviderReadDTO> providers = await providerService.GetAllAsync(cancellationToken);

                foreach (DeliveryProviderReadDTO provider in providers)
                {
                    string cacheKey = $"delivery-provider:{provider.Id}";
                    await cacheService.SetAsync(cacheKey, provider);
                    logger.LogInformation("Preloaded DeliveryProvider {Name} ({Id}) into cache with key {CacheKey}", provider.Name, provider.Id, cacheKey);
                }

                logger.LogInformation("DeliveryProviderCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DeliveryProviderCachePreloader failed during cache warming.");
            }
        }
    }
}
