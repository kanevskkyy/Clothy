using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.BLL.RedisCache.OrdersCache
{
    public class OrderCachePreloader : ICachePreloader
    {
        private IEntityCacheService cacheService;
        private IOrderService orderService;
        private ILogger<OrderCachePreloader> logger;
        private IUnitOfWork unitOfWork;
        private static TimeSpan MEMORY_TTL = TimeSpan.FromMinutes(30);
        private static TimeSpan REDIS_TTL = TimeSpan.FromHours(2);

        private const int PAGE_SIZE = 10;
        private const int TOTAL_PAGES = 3;

        public OrderCachePreloader(IEntityCacheService cacheService, IOrderService orderService, ILogger<OrderCachePreloader> logger, IUnitOfWork unitOfWork)
        {
            this.cacheService = cacheService;
            this.orderService = orderService;
            this.logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public async Task PreloadAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting OrderCachePreloader...");

            try
            {
                for (int page = 1; page <= TOTAL_PAGES; page++)
                {
                    OrderStatus? pendingStatus = await unitOfWork.OrderStatuses.GetByNameAsync("Pending", cancellationToken);

                    OrderFilterDTO filter = new OrderFilterDTO
                    {
                        PageNumber = page,
                        PageSize = PAGE_SIZE,
                        StatusId = pendingStatus.Id
                    };

                    PagedList<OrderReadDTO> pagedResult = await orderService.GetPagedAsync(filter, cancellationToken);
                    if (pagedResult == null)
                    {
                        logger.LogWarning("Paged orders is null for page {Page}. Skipping cache.", page);
                        continue;
                    }

                    string cacheKey = $"orders:status:{pendingStatus.Id}:page:{page}:size:{PAGE_SIZE}";
                    await cacheService.SetAsync(cacheKey, pagedResult, MEMORY_TTL, REDIS_TTL);

                    logger.LogInformation("Preloaded Pending Orders, page {Page}, {Count} items into cache with key {CacheKey}.",
                        page, pagedResult.Items.Count, cacheKey);
                }

                logger.LogInformation("OrderCachePreloader completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OrderCachePreloader failed during cache warming.");
            }
        }
    }
}
