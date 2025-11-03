using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.CatalogService.BLL.Services
{
    public class ClothesStockService : IClothesStockService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<ClothesStock> cacheInvalidationService;
        private static TimeSpan MEMORY_TTL_PAGE = TimeSpan.FromMinutes(1);
        private static TimeSpan REDIS_TTL_PAGE = TimeSpan.FromMinutes(10);

        private static TimeSpan MEMORY_TTL_ITEM = TimeSpan.FromMinutes(5);
        private static TimeSpan REDIS_TTL_ITEM = TimeSpan.FromMinutes(30);

        public ClothesStockService(IUnitOfWork unitOfWork, IMapper mapper, IEntityCacheService cacheService, IEntityCacheInvalidationService<ClothesStock> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<PagedList<ClothesStockReadDTO>> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            bool shouldCache = parameters.PageNumber <= 3;
            string cacheKey = $"clothesstock:page:{parameters.PageNumber}:size:{parameters.PageSize}";

            if (shouldCache)
            {
                var cached = await cacheService.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        PagedList<ClothesStock> paged = await unitOfWork.ClothesStocks.GetPagedClothesStockAsync(parameters, cancellationToken);
                        List<ClothesStockReadDTO> mapped = mapper.Map<List<ClothesStockReadDTO>>(paged.Items);
                        return new PagedList<ClothesStockReadDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
                    },
                    memoryExpiration: MEMORY_TTL_PAGE,
                    redisExpiration: REDIS_TTL_PAGE
                );

                return cached!;
            }
            else
            {
                PagedList<ClothesStock> paged = await unitOfWork.ClothesStocks.GetPagedClothesStockAsync(parameters, cancellationToken);
                List<ClothesStockReadDTO> mapped = mapper.Map<List<ClothesStockReadDTO>>(paged.Items);
                return new PagedList<ClothesStockReadDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
            }
        }

        public async Task<ClothesStockReadDTO> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"clothesstock:{id}";

            var cached = await cacheService.GetOrSetAsync(
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

        public async Task<ClothesStockReadDTO> CreateAsync(ClothesStockCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.ClothesStocks.IsSizeAndColorAndClotheIdsExists(dto.SizeId, dto.ColorId, dto.ClotheId, cancellationToken);
            if (exists) throw new AlreadyExistsException("Clothes stock with this Size, Color and Clothe already exists");

            ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdAsync(dto.ClotheId, cancellationToken);
            if (clotheItem == null) throw new NotFoundException($"ClotheItem not found with ID: {dto.ClotheId}");

            Size? size = await unitOfWork.Sizes.GetByIdAsync(dto.SizeId, cancellationToken);
            if (size == null) throw new NotFoundException($"Size not found with ID: {dto.SizeId}");

            Color? color = await unitOfWork.Colors.GetByIdAsync(dto.ColorId, cancellationToken);
            if (color == null) throw new NotFoundException($"Color not found with ID: {dto.ColorId}");

            ClothesStock stock = mapper.Map<ClothesStock>(dto);
            await unitOfWork.ClothesStocks.AddAsync(stock, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            await cacheInvalidationService.InvalidateAllAsync();
            return await GetByIdWithDetailsAsync(stock.Id, cancellationToken);
        }

        public async Task<ClothesStockReadDTO> UpdateAsync(Guid id, ClothesStockUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            mapper.Map(dto, stock);
            unitOfWork.ClothesStocks.Update(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheInvalidationService.InvalidateByIdAsync(id);
            return await GetByIdWithDetailsAsync(stock.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            unitOfWork.ClothesStocks.Delete(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheInvalidationService.InvalidateByIdAsync(id);
        }
    }
}