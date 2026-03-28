using Clothy.OrderService.BLL.DTOs.APIClientDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.Services.BackgroundServices
{
    public class PickupPointSyncBackgroundService : BackgroundService
    {
        private ILogger<PickupPointSyncBackgroundService> logger;
        private IServiceProvider serviceProvider;

        private TimeSpan SYNC_TIME = new TimeSpan(3, 0, 0);
        private const int MAX_PARALLEL_REGIONS = 3;

        public PickupPointSyncBackgroundService(
            ILogger<PickupPointSyncBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("PickupPoint Sync Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan timeUntilSync = CalculateTimeUntilNextSync();
                logger.LogInformation("Next sync scheduled in {Hours} hours and {Minutes} minutes", timeUntilSync.Hours, timeUntilSync.Minutes);

                await Task.Delay(timeUntilSync, stoppingToken);
                await PerformSyncAsync(stoppingToken);
            }
        }

        private TimeSpan CalculateTimeUntilNextSync()
        {
            DateTime now = DateTime.UtcNow;
            DateTime nextSync = now.Date.Add(SYNC_TIME);

            if (now >= nextSync) nextSync = nextSync.AddDays(1);
            return nextSync - now;
        }

        private async Task PerformSyncAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting PickupPoint synchronization at {Time}", DateTime.UtcNow);

            try
            {
                using IServiceScope serviceScope = serviceProvider.CreateScope();
                IUnitOfWork unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IDeliveryAPIClient deliveryAPIClient = serviceScope.ServiceProvider.GetRequiredService<IDeliveryAPIClient>();
                IEntityCacheInvalidationService<PickupPoints> entityCacheInvalidationService = serviceScope.ServiceProvider.GetRequiredService<IEntityCacheInvalidationService<PickupPoints>>();

                DeliveryProvider? deliveryProvider = await unitOfWork.DeliveryProviders.GetByNameAsync("NovaPoshta", cancellationToken);

                if (deliveryProvider == null)
                {
                    logger.LogWarning("NovaPoshta delivery provider not found. Skipping sync.");
                    return;
                }

                logger.LogInformation("Fetching regions from NovaPoshta API");
                List<RegionDTO> regionDTOs = await deliveryAPIClient.GetAreasAsync();
                logger.LogInformation("Found {Count} regions to process", regionDTOs.Count);

                int totalAdded = 0;
                int totalDeactivated = 0;

                await Parallel.ForEachAsync(
                    regionDTOs,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = MAX_PARALLEL_REGIONS,
                        CancellationToken = cancellationToken
                    },
                    async (regionDTO, ct) =>
                    {
                        var (added, deactivated) = await ProcessRegionAsync(regionDTO, deliveryProvider.Id, ct);
                        Interlocked.Add(ref totalAdded, added);
                        Interlocked.Add(ref totalDeactivated, deactivated);
                    }
                );

                if (totalAdded > 0 || totalDeactivated > 0)
                {
                    await entityCacheInvalidationService.InvalidateAllAsync();
                    logger.LogInformation("Cache invalidated due to changes");
                }

                logger.LogInformation("Synchronization completed. Added: {Added}, Deactivated: {Deactivated}", totalAdded, totalDeactivated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during PickupPoint synchronization");
            }
        }

        private async Task<(int added, int deactivated)> ProcessRegionAsync(RegionDTO regionDTO, Guid deliveryProviderId, CancellationToken cancellationToken)
        {
            logger.LogDebug("Processing region: {Region}", regionDTO.Description);

            int addedCount = 0;
            int deactivatedCount = 0;

            try
            {
                using IServiceScope serviceScope = serviceProvider.CreateScope();
                IUnitOfWork unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                IDeliveryAPIClient deliveryAPIClient = serviceScope.ServiceProvider.GetRequiredService<IDeliveryAPIClient>();
                IEntityCacheInvalidationService<PickupPoints> entityCacheInvalidationService = serviceScope.ServiceProvider.GetRequiredService<IEntityCacheInvalidationService<PickupPoints>>();

                Region? region = await unitOfWork.Region.GetByRefAsync(regionDTO.Ref!, cancellationToken);

                if (region == null)
                {
                    logger.LogWarning("Region {Ref} not found in database. Skipping.", regionDTO.Ref);
                    return (0, 0);
                }

                List<SettlementDTO> settlementDTOs = await deliveryAPIClient.GetSettlementsByAreaRefAsync(region.Ref!);

                foreach (SettlementDTO settlementDTO in settlementDTOs)
                {
                    var (added, deactivated) = await ProcessSettlementAsync(
                        settlementDTO,
                        region.Id,
                        deliveryProviderId,
                        unitOfWork,
                        deliveryAPIClient,
                        entityCacheInvalidationService,
                        cancellationToken
                    );

                    addedCount += added;
                    deactivatedCount += deactivated;
                }

                await unitOfWork.CommitAsync();

                logger.LogDebug("Region {Region} completed. Added: {Added}, Deactivated: {Deactivated}", regionDTO.Description, addedCount, deactivatedCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing region {Region}", regionDTO.Description);
            }

            return (addedCount, deactivatedCount);
        }

        private async Task<(int added, int deactivated)> ProcessSettlementAsync(SettlementDTO settlementDTO, Guid regionId, Guid deliveryProviderId, IUnitOfWork unitOfWork, IDeliveryAPIClient deliveryAPIClient, IEntityCacheInvalidationService<PickupPoints> entityCacheInvalidationService, CancellationToken cancellationToken = default)
        {
            int addedCount = 0;
            int deactivatedCount = 0;

            try
            {
                Settlement? settlement = await unitOfWork.Settlement.GetByRefAsync(settlementDTO.Ref!, cancellationToken);

                if (settlement == null)
                {
                    logger.LogWarning("Settlement {Ref} not found. Skipping.", settlementDTO.Ref);
                    return (0, 0);
                }

                List<PickupPointDTO> apiPickupPoints = await deliveryAPIClient.GetWarehousesByCityRefAsync(settlement.Ref!);
                HashSet<string>? apiResults = apiPickupPoints.Select(p => p.Ref).ToHashSet()!;
                List<PickupPoints> existingPickupPoints = await unitOfWork.PickupPoint.GetBySettlementIdAsync(settlement.Id, cancellationToken);

                foreach (PickupPointDTO pickupPointDTO in apiPickupPoints)
                {
                    PickupPoints? pickupPoint = existingPickupPoints.FirstOrDefault(p => p.Ref == pickupPointDTO.Ref);

                    if (pickupPoint == null)
                    {
                        PickupPoints newPickupPoint = new PickupPoints
                        {
                            Address = pickupPointDTO.Description,
                            Ref = pickupPointDTO.Ref,
                            IsActive = true,
                            DeliveryProviderId = deliveryProviderId,
                            SettlementId = settlement.Id
                        };

                        await unitOfWork.PickupPoint.AddAsync(newPickupPoint, cancellationToken);
                        addedCount++;

                        logger.LogDebug("Added new pickup point: {Address}", pickupPointDTO.Description);
                    }
                    else if (!pickupPoint.IsActive)
                    {
                        pickupPoint.IsActive = true;
                        pickupPoint.Address = pickupPointDTO.Description;
                        await unitOfWork.PickupPoint.UpdateAsync(pickupPoint);
                        await entityCacheInvalidationService.InvalidateAllAsync();
                        await entityCacheInvalidationService.InvalidateByIdAsync(pickupPoint.Id);
                        addedCount++;

                        logger.LogDebug("Reactivated pickup point: {Address}", pickupPointDTO.Description);
                    }
                }

                foreach (PickupPoints tempPickupPoint in existingPickupPoints.Where(p => p.IsActive))
                {
                    if (!apiResults.Contains(tempPickupPoint.Ref!))
                    {
                        tempPickupPoint.IsActive = false;
                        await unitOfWork.PickupPoint.UpdateAsync(tempPickupPoint);
                        deactivatedCount++;
                        await entityCacheInvalidationService.InvalidateAllAsync();
                        await entityCacheInvalidationService.InvalidateByIdAsync(tempPickupPoint.Id);

                        logger.LogDebug("Deactivated pickup point: {Address}", tempPickupPoint.Address);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing settlement {Settlement}", settlementDTO.Description);
            }

            return (addedCount, deactivatedCount);
        }
    }
}