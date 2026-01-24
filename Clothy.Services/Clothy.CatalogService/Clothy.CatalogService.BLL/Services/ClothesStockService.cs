using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.Services
{
    public class ClothesStockService : IClothesStockService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<ClothesStock> cacheInvalidationService;
        private IFilterCacheInvalidationService filterCacheInvalidationService;
        private IStockNotificationService stockNotificationService;

        private static TimeSpan MEMORY_TTL_PAGE = TimeSpan.FromMinutes(1);
        private static TimeSpan REDIS_TTL_PAGE = TimeSpan.FromMinutes(10);

        private static TimeSpan MEMORY_TTL_ITEM = TimeSpan.FromMinutes(5);
        private static TimeSpan REDIS_TTL_ITEM = TimeSpan.FromMinutes(30);

        private Counter<long> stockUpdatedCounter;

        public ClothesStockService(IUnitOfWork unitOfWork,
            IMapper mapper,
            IEntityCacheService cacheService,
            IEntityCacheInvalidationService<ClothesStock> cacheInvalidationService,
            Meter meter,
            IFilterCacheInvalidationService filterCacheInvalidationService,
            IStockNotificationService stockNotificationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
            stockUpdatedCounter = meter.CreateCounter<long>(
                "clothy.catalog.clotheStock.updated",
                "items",
                "Number of stock changes (add/update)");
            this.filterCacheInvalidationService = filterCacheInvalidationService;
            this.stockNotificationService = stockNotificationService;
        }

        public async Task<PagedList<ClothesStockReadDTO>?> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            bool shouldCache = parameters.PageNumber <= 3;

            if (shouldCache)
            {
                return await cacheService.GetOrSetAsync(
                    parameters.ToCacheKey(),
                    async () => await FetchClothesStockAsync(parameters, cancellationToken),
                    memoryExpiration: MEMORY_TTL_PAGE,
                    redisExpiration: REDIS_TTL_PAGE
                );
            }

            return await FetchClothesStockAsync(parameters, cancellationToken);
        }

        private async Task<PagedList<ClothesStockReadDTO>> FetchClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken)
        {
            PagedList<ClothesStock> paged = await unitOfWork.ClothesStocks.GetPagedClothesStockAsync(parameters, cancellationToken);
            List<ClothesStockReadDTO> mapped = mapper.Map<List<ClothesStockReadDTO>>(paged.Items);

            return new PagedList<ClothesStockReadDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<ClothesStockReadDTO> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"clothesstock:{id}";

            ClothesStockReadDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
                    if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");
                    return mapper.Map<ClothesStockReadDTO>(stock);
                },
                memoryExpiration: MEMORY_TTL_ITEM,
                redisExpiration: REDIS_TTL_ITEM
            );

            return cached!;
        }

        public async Task<ClothesStockReadDTO> CreateAsync(ClothesStockCreateDTO clothesStockCreateDTO, CancellationToken cancellationToken = default)
        {
            ClothesStock? clothesStock = await unitOfWork.ClothesStocks.GetByClotheColorSizeAsync(clothesStockCreateDTO.ClotheId, clothesStockCreateDTO.ColorId, clothesStockCreateDTO.SizeId, cancellationToken);
            if (clothesStock != null) throw new AlreadyExistsException("Clothes stock with this Size, Color and Clothe already exists");

            ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdAsync(clothesStockCreateDTO.ClotheId, cancellationToken);
            if (clotheItem == null) throw new NotFoundException($"ClotheItem not found with ID: {clothesStockCreateDTO.ClotheId}");

            Size? size = await unitOfWork.Sizes.GetByIdAsync(clothesStockCreateDTO.SizeId, cancellationToken);
            if (size == null) throw new NotFoundException($"Size not found with ID: {clothesStockCreateDTO.SizeId}");

            Color? color = await unitOfWork.Colors.GetByIdAsync(clothesStockCreateDTO.ColorId, cancellationToken);
            if (color == null) throw new NotFoundException($"Color not found with ID: {clothesStockCreateDTO.ColorId}");

            ClothesStock stock = mapper.Map<ClothesStock>(clothesStockCreateDTO);
            await unitOfWork.ClothesStocks.AddAsync(stock, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            stockUpdatedCounter.Add(1, new KeyValuePair<string, object?>("operation", "create"));

            await cacheService.RemoveAsync($"clothe:{clotheItem.Slug}");
            await cacheInvalidationService.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();

            return await GetByIdWithDetailsAsync(stock.Id, cancellationToken);
        }

        public async Task<ClothesStockReadDTO> UpdateAsync(Guid id, ClothesStockUpdateDTO clothesStockUpdateDTO, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            int startQuantity = stock.Quantity;

            mapper.Map(clothesStockUpdateDTO, stock);
            unitOfWork.ClothesStocks.Update(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (startQuantity == 0 && stock.Quantity > 0)
            {
                await stockNotificationService.NotifySubscribersAsync(id, cancellationToken);
            }

            stockUpdatedCounter.Add(1, new KeyValuePair<string, object?>("operation", "update"));

            await cacheInvalidationService.InvalidateAllAsync();
            await cacheInvalidationService.InvalidateByIdAsync(id);

            await cacheService.RemoveAsync($"clothe:{stock.Clothe.Slug}");
            await filterCacheInvalidationService.InvalidateAsync();

            return await GetByIdWithDetailsAsync(stock.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            unitOfWork.ClothesStocks.Delete(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync($"clothe:{stock.Clothe.Slug}");

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();

            await filterCacheInvalidationService.InvalidateAsync();
        }

        public async Task UpdateStockAsync(Guid clotheId, Guid colorId, Guid sizeId, int orderedQuantity, CancellationToken cancellationToken = default)
        {
            ClothesStock? clotheStock = await unitOfWork.ClothesStocks.GetByClotheColorSizeAsync(clotheId, colorId, sizeId, cancellationToken);

            clotheStock.Quantity -= orderedQuantity;
            unitOfWork.ClothesStocks.Update(clotheStock);

            ClothePopularity? clothePopularity = await unitOfWork.ClothePopularity.GetClothePopularityByClotheIdAsync(clotheId, cancellationToken);
            if (clothePopularity != null)
            {
                clothePopularity.SoldCount += orderedQuantity;
                clothePopularity.UpdatedAt = DateTime.UtcNow.ToUniversalTime();
                unitOfWork.ClothePopularity.Update(clothePopularity);
            }
            else
            {
                ClothePopularity popularity = new ClothePopularity
                {
                    ClotheId = clotheId,
                    SoldCount = orderedQuantity,
                    CreatedAt = DateTime.UtcNow.ToUniversalTime()
                };
                await unitOfWork.ClothePopularity.AddAsync(popularity);
            }

            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveAsync($"clothe:{clotheStock.Clothe.Slug}");
            await cacheService.RemoveAsync("clothe:top8_most_popular");

            await cacheInvalidationService.InvalidateAllAsync();
            await cacheInvalidationService.InvalidateByIdAsync(clotheStock.Id);

            await filterCacheInvalidationService.InvalidateAsync();
        }
    }
}