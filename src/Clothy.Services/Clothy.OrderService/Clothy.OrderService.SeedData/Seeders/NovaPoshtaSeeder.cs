using Bogus;
using Clothy.OrderService.BLL.DTOs.APIClientDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.DAL.Interfaces;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Clothy.OrderService.SeedData.Seeders
{
    public class NovaPoshtaSeeder : ISeeder
    {
        private IDeliveryAPIClient deliveryAPIClient;
        private IServiceProvider serviceProvider;
        private const int MAX_PARALLEL_REGIONS = 3; 

        public NovaPoshtaSeeder(IDeliveryAPIClient client, IServiceProvider serviceProvider)
        {
            deliveryAPIClient = client;
            this.serviceProvider = serviceProvider;
        }

        public async Task SeedAsync(IUnitOfWork unitOfWork)
        {
            Faker faker = new Faker();

            bool exist = await unitOfWork.DeliveryProviders.ExistsByNameAsync("NovaPoshta");
            if (exist)
            {
                Console.WriteLine("NovaPoshta already exists, skipping...");
                return;
            }

            DeliveryProvider deliveryProvider = new DeliveryProvider()
            {
                Name = "NovaPoshta",
                IconUrl = "https://res.cloudinary.com/dkdljnfja/image/upload/v1772204809/unnamed_apw8b3.jpg",
                CreatedAt = DateTime.UtcNow
            };
            deliveryProvider.Id = await unitOfWork.DeliveryProviders.AddAsync(deliveryProvider);
            await unitOfWork.CommitAsync();

            Console.WriteLine("Fetching regions...");
            List<RegionDTO> regionDTOs = await deliveryAPIClient.GetAreasAsync();
            Console.WriteLine($"Found {regionDTOs.Count} regions. Starting parallel processing...");

            await Parallel.ForEachAsync(
                regionDTOs,
                new ParallelOptions { MaxDegreeOfParallelism = MAX_PARALLEL_REGIONS },
                async (regionDTO, ct) =>
                {
                    using IServiceScope scope = serviceProvider.CreateScope();
                    IUnitOfWork threadUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await ProcessRegionAsync(regionDTO, deliveryProvider.Id, threadUnitOfWork);
                }
            );

            Console.WriteLine("All regions processed!");
        }

        private async Task ProcessRegionAsync(RegionDTO regionDTO, Guid deliveryProviderId, IUnitOfWork unitOfWork)
        {
            Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] Processing region: {regionDTO.Description}");

            Region region = new Region()
            {
                Name = regionDTO.Description,
                Ref = regionDTO.Ref,
                CreatedAt = DateTime.UtcNow
            };
            region.Id = await unitOfWork.Region.AddAsync(region);
            await unitOfWork.CommitAsync();

            List<SettlementDTO> settlementDTOs = await deliveryAPIClient.GetSettlementsByAreaRefAsync(region.Ref!);
            Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] Region {regionDTO.Description}: Found {settlementDTOs.Count} settlements");

            var settlementBatches = settlementDTOs
                .Select((s, i) => new { Settlement = s, Index = i })
                .GroupBy(x => x.Index / 30) 
                .Select(g => g.Select(x => x.Settlement).ToList());

            int totalProcessed = 0;
            int batchNumber = 0;

            foreach (var batch in settlementBatches)
            {
                batchNumber++;

                var tasks = batch.Select(settlementDTO =>
                    ProcessSettlementAsync(settlementDTO, region.Id, deliveryProviderId, unitOfWork)
                );

                await Task.WhenAll(tasks);

                totalProcessed += batch.Count;

                await unitOfWork.CommitAsync();
                Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] Region {regionDTO.Description}: Batch {batchNumber} - {totalProcessed}/{settlementDTOs.Count} settlements");
            }

            Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] Region {regionDTO.Description}: COMPLETED ({settlementDTOs.Count} settlements)");
        }

        private async Task ProcessSettlementAsync(SettlementDTO settlementDTO, Guid regionId, Guid deliveryProviderId, IUnitOfWork unitOfWork)
        {
            Settlement settlement = new Settlement()
            {
                Name = settlementDTO.Description,
                Ref = settlementDTO.Ref,
                Type = GetSettlementTypeByName(settlementDTO.SettlementTypeDescription!),
                RegionId = regionId
            };
            settlement.Id = await unitOfWork.Settlement.AddAsync(settlement);

            try
            {
                List<PickupPointDTO> pickupPointDTOs = await deliveryAPIClient.GetWarehousesByCityRefAsync(settlement.Ref!);

                foreach (PickupPointDTO pickupPointDTO in pickupPointDTOs)
                {
                    PickupPoints pickupPoints = new PickupPoints()
                    {
                        Address = pickupPointDTO.Description,
                        Ref = pickupPointDTO.Ref,
                        IsActive = true,
                        DeliveryProviderId = deliveryProviderId,
                        SettlementId = settlement.Id
                    };
                    await unitOfWork.PickupPoint.AddAsync(pickupPoints);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing settlement {settlement.Name}: {ex.Message}");
            }
        }

        private SettlementType GetSettlementTypeByName(string settlementTypeDescription)
        {
            if (settlementTypeDescription.ToLower() == "село") return SettlementType.Village;
            else if (settlementTypeDescription.ToLower() == "селище міського типу") return SettlementType.Urban;

            return SettlementType.City;
        }
    }
}