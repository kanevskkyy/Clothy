using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.OrderStatusCache
{
    public class OrderStatusCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private IOrderStatusService orderStatusService;
        private ILogger<OrderStatusCachePreloader> logger;

        public OrderStatusCachePreloader(IEntityCacheService cacheService, IOrderStatusService orderStatusService, ILogger<OrderStatusCachePreloader> logger)
        {
            this.cacheService = cacheService;
            this.orderStatusService = orderStatusService;
            this.logger = logger;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting OrderStatusCachePreloader...");

            try
            {
                List<OrderStatusReadDTO> statuses = await orderStatusService.GetAllAsync(cancellationToken);

                foreach (OrderStatusReadDTO status in statuses)
                {
                    string cacheKey = $"order-status:{status.Id}";
                    await cacheService.SetAsync(cacheKey, status);
                    logger.LogInformation("Preloaded OrderStatus {Name} ({Id}) into cache with key {CacheKey}", status.Name, status.Id, cacheKey);
                }

                logger.LogInformation("OrderStatusCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OrderStatusCachePreloader failed during cache warming.");
            }
        }
    }
}
