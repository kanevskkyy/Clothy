using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.Services.BackgroundServices
{
    public class ExpiredOrdersCleanupService : BackgroundService
    {
        private ILogger<ExpiredOrdersCleanupService> logger;
        private IServiceScopeFactory scopeFactory;
        private TimeSpan INTERVAL = TimeSpan.FromMinutes(1);
        private IEntityCacheInvalidationService<Order> cacheInvalidationService;    

        public ExpiredOrdersCleanupService(
            ILogger<ExpiredOrdersCleanupService> logger,
            IServiceScopeFactory scopeFactory,
            IEntityCacheInvalidationService<Order> cacheInvalidationService)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ExpiredOrdersCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    List<OrderReservation> expiredReservations = await unitOfWork.OrderReservation.GetExpiredReservationsAsync(stoppingToken);

                    if (expiredReservations.Count > 0) logger.LogInformation("Found {Count} expired reservations for unpaid orders", expiredReservations.Count);

                    var groupedByOrder = expiredReservations.GroupBy(r => r.OrderId);

                    foreach (var orderGroup in groupedByOrder)
                    {
                        Guid orderId = orderGroup.Key;

                        foreach (OrderReservation reservation in orderGroup)
                        {
                            reservation.IsActive = false;
                            await unitOfWork.OrderReservation.UpdateAsync(reservation, stoppingToken);
                        }

                        await unitOfWork.Orders.DeleteAsync(orderId, stoppingToken);
                        await cacheInvalidationService.InvalidateByIdAsync(orderId);

                        logger.LogInformation("Deleted expired unpaid order {OrderId} with {ReservationCount} reservations", orderId, orderGroup.Count());
                    }

                    if (expiredReservations.Count > 0)
                    {
                        await unitOfWork.CommitAsync();
                        await cacheInvalidationService.InvalidateAllAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while cleaning expired orders");
                }

                await Task.Delay(INTERVAL, stoppingToken);
            }

            logger.LogInformation("ExpiredOrdersCleanupService stopping.");
        }
    }
}
