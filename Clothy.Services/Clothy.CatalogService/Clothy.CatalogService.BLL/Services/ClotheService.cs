using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Helpers;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Helpers;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Clothy.Shared.Events.ClotheItemEvents;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;

namespace Clothy.CatalogService.BLL.Services
{
    public class ClotheService : IClotheService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<ClotheItem> clotheItemInvalidationService;
        private IPublishEndpoint publishEndpoint;
        private IEntityCacheInvalidationService<ClothesStock> clotheStockInvalidationService;
        private IFilterCacheInvalidationService filterCacheInvalidationService;

        private const int MAX_CASHED_PAGES = 10;
        private static TimeSpan MEMORY_TTL_CLOTHE_PAGE = TimeSpan.FromMinutes(30);
        private static TimeSpan REDIS_TTL_CLOTHE_PAGE = TimeSpan.FromHours(1);
        
        private static TimeSpan MEMORY_TTL_CLOTHE_DETAIL = TimeSpan.FromMinutes(15);
        private static TimeSpan REDIS_TTL_CLOTHE_DETAIL = TimeSpan.FromMinutes(30);
        
        private Counter<long> clotheCreatedMetric;

        public ClotheService(IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IImageService imageService, 
            IEntityCacheService cacheService, 
            IEntityCacheInvalidationService<ClotheItem> cacheInvalidationService, 
            Meter meter,
            IEntityCacheInvalidationService<ClothesStock> cacheInvalidatonClotheStock, 
            IPublishEndpoint publishEndpoint,
            IFilterCacheInvalidationService filterCacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
            this.cacheService = cacheService;
            this.clotheItemInvalidationService = cacheInvalidationService;
            clotheCreatedMetric = meter.CreateCounter<long>(
                "clothy.catalog.clotheItem.created_total",
                "items",
                "Total numbers of clothes created");
            this.clotheStockInvalidationService = cacheInvalidatonClotheStock;
            this.publishEndpoint = publishEndpoint;
            this.filterCacheInvalidationService = filterCacheInvalidationService;
        }

        public async Task<PagedList<ClotheSummaryDTO>?> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            bool usePageCache = parameters.PageNumber <= MAX_CASHED_PAGES;

            if (usePageCache)
            {
                return await cacheService.GetOrSetAsync(
                    parameters.ToCacheKey(),
                    async () => await FetchClotheItemsAsync(parameters, cancellationToken),
                    MEMORY_TTL_CLOTHE_PAGE,
                    REDIS_TTL_CLOTHE_PAGE
                );
            }

            return await FetchClotheItemsAsync(parameters, cancellationToken);
        }

        private async Task<PagedList<ClotheSummaryDTO>> FetchClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken)
        {
            PagedList<ClotheItem> paged = await unitOfWork.ClotheItems.GetPagedClotheItemsAsync(parameters, cancellationToken);
            List<ClotheSummaryDTO> mapped = mapper.Map<List<ClotheSummaryDTO>>(paged.Items);
            
            return new PagedList<ClotheSummaryDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<ClotheDetailDTO> GetDetailBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"clothe:{slug}";
            ClotheDetailDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetBySlugWithDetailsAsync(slug, cancellationToken);
                    if (clotheItem == null) throw new NotFoundException($"Clothe item not found with slug: {slug}");
                    return mapper.Map<ClotheDetailDTO>(clotheItem);
                },
                MEMORY_TTL_CLOTHE_DETAIL,
                REDIS_TTL_CLOTHE_DETAIL
            );

            return cached!;
        }

        public async Task<ClotheDetailDTO> CreateAsync(ClotheCreateDTO clotheCreateDTO, CancellationToken cancellationToken = default)
        {
            if (await unitOfWork.ClotheItems.IsSlugAlreadyExistsAsync(clotheCreateDTO.Slug, null, cancellationToken)) throw new AlreadyExistsException("Clothe with this slug already exists");

            List<ClotheMaterialCreateDTO> distinctMaterials = clotheCreateDTO.Materials
                .GroupBy(m => m.MaterialId)
                .Select(g => g.First())
                .ToList();

            int totalPercentage = distinctMaterials.Sum(p => p.Percentage);
            if (totalPercentage != 100) throw new InvalidMaterialPercentageException("Total material percentage must be exactly 100.");

            Brand? brand = await unitOfWork.Brands.GetByIdAsync(clotheCreateDTO.BrandId, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand with ID: {clotheCreateDTO.BrandId} not found");

            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(clotheCreateDTO.ClothingTypeId, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"Clothing type with ID: {clotheCreateDTO.ClothingTypeId} not found");

            Collection? collection = await unitOfWork.Collections.GetByIdAsync(clotheCreateDTO.CollectionId, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection with ID: {clotheCreateDTO.CollectionId} not found");

            ClotheItem clothe = mapper.Map<ClotheItem>(clotheCreateDTO);

            bool isDuplicated = clotheCreateDTO.AdditionalPhotos
                .Where(p => p.IsMain)
                .GroupBy(p => p.ColorId)
                .Any(g => g.Count() > 1);

            if (isDuplicated) throw new ValidationFailedException("Only one main photo per color is allowed.");

            clothe.Photos = new List<PhotoClothes>();
            
            var colorGroups = clotheCreateDTO.AdditionalPhotos
                .Where(p => p.ColorId != null)
                .GroupBy(p => p.ColorId);
            
            foreach (var group in colorGroups)
            {
                int mainCount = group.Count(p => p.IsMain);

                if (mainCount == 0) throw new ValidationFailedException($"Color with ID {group.Key} must have at least one main photo.");
                if (mainCount > 1) throw new ValidationFailedException($"Color with ID {group.Key} cannot have more than one main photo.");
            }

            IReadOnlyList<Size> sizes = await unitOfWork.Sizes.GetAllAsync(cancellationToken);
            List<Guid> distinctColorIds = new List<Guid>();

            foreach (ClothePhotoCreateDTO clothePhotoCreateDTO in clotheCreateDTO.AdditionalPhotos)
            {
                string url = await imageService.UploadAsync(clothePhotoCreateDTO.Photo, "clothes");
                Color? color = await unitOfWork.Colors.GetByIdAsync(clothePhotoCreateDTO.ColorId, cancellationToken);
                if (color == null) throw new NotFoundException($"Color not found with ID: {clothePhotoCreateDTO.ColorId}");

                if(!distinctColorIds.Contains(color.Id))
                {
                    distinctColorIds.Add(color.Id);

                    foreach(Size size in sizes)
                    {
                        clothe.Stocks.Add(new ClothesStock
                        {
                            SizeId = size.Id,
                            ColorId = color.Id,
                            Quantity = 0
                        });
                    }
                }

                clothe.Photos.Add(new PhotoClothes
                {
                    PhotoURL = url,
                    ColorId = clothePhotoCreateDTO.ColorId,
                    IsMain = clothePhotoCreateDTO.IsMain
                });
            }

            if (!await unitOfWork.Materials.AreAllExistAsync(distinctMaterials.Select(material => material.MaterialId), cancellationToken)) throw new NotFoundException("One or more materials do not exist.");
            if (!await unitOfWork.Tags.AreAllExistAsync(clotheCreateDTO.TagIds, cancellationToken)) throw new NotFoundException("One or more tags do not exist.");

            List<Guid> distinctTags = clotheCreateDTO.TagIds.Distinct().ToList();

            clothe.ClotheMaterials = distinctMaterials
                .Select(clotheMaterial => new ClotheMaterial 
                { 
                    MaterialId = clotheMaterial.MaterialId, 
                    Percentage = clotheMaterial.Percentage 
                })
                .ToList();

            clothe.ClotheTags = distinctTags
                .Select(tagId => new ClotheTag { 
                    TagId = tagId 
                })
                .ToList();

            await unitOfWork.ClotheItems.AddAsync(clothe, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            clotheCreatedMetric.Add(1, new KeyValuePair<string, object?>("collection", collection.Name),
                              new KeyValuePair<string, object?>("clotheType", clothingType.Name));

            await clotheItemInvalidationService.InvalidateAllAsync();
            await clotheStockInvalidationService.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();
            await cacheService.RemoveAsync("clothe:top8_most_popular");

            return await GetDetailBySlugAsync(clothe.Slug, cancellationToken);
        }

        public async Task<ClotheDetailDTO> UpdateAsync(Guid id, ClotheUpdateDTO clotheUpdateDTO, CancellationToken cancellationToken = default)
        {
            ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdAsync(id, cancellationToken);
            if (clotheItem == null) throw new NotFoundException($"Clothe item not found with ID: {id}");

            if (await unitOfWork.ClotheItems.IsSlugAlreadyExistsAsync(clotheUpdateDTO.Slug, id, cancellationToken)) throw new AlreadyExistsException("Clothe with this slug already exists");

            Brand? brand = await unitOfWork.Brands.GetByIdAsync(clotheUpdateDTO.BrandId, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand with ID: {clotheUpdateDTO.BrandId} not found");

            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(clotheUpdateDTO.ClothingTypeId, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"Clothing type with ID: {clotheUpdateDTO.ClothingTypeId} not found");

            Collection? collection = await unitOfWork.Collections.GetByIdAsync(clotheUpdateDTO.CollectionId, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection with ID: {clotheUpdateDTO.CollectionId} not found");

            decimal currentPrice = clotheItem.Price;
            decimal? currentOldPrice = clotheItem.OldPrice;

            await cacheService.RemoveAsync($"clothe:{clotheItem.Slug}");
            mapper.Map(clotheUpdateDTO, clotheItem);

            if (clotheItem.Price < currentPrice)
            {
                if (!currentOldPrice.HasValue) clotheItem.OldPrice = currentPrice;
            }
            else if (clotheItem.Price >= currentOldPrice)
            {
                clotheItem.OldPrice = null;
            }

            unitOfWork.ClotheItems.Update(clotheItem);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await clotheItemInvalidationService.InvalidateAllAsync();

            ClotheItemUpdatedEvent clotheItemUpdatedEvent = new ClotheItemUpdatedEvent
            {
                ClotheId = clotheItem.Id,
                ClotheName = clotheItem.Name,
                Price = clotheItem.Price,
            };
            await publishEndpoint.Publish(clotheItemUpdatedEvent, cancellationToken);
            await clotheStockInvalidationService.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();
            await cacheService.RemoveAsync("clothe:top8_most_popular");

            return await GetDetailBySlugAsync(clotheItem.Slug, cancellationToken);
        }

        public async Task<PriceRangeDTO> GetMinAndMaxPriceAsync(CancellationToken cancellationToken = default)
        {
            (decimal minPrice, decimal maxPrice) = await unitOfWork.ClotheItems.GetMinAndMaxPriceAsync(cancellationToken);
           
            return new PriceRangeDTO { 
                MinPrice = minPrice, 
                MaxPrice = maxPrice 
            };
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdWithDetailsAsync(id, cancellationToken);
            if (clotheItem == null) throw new NotFoundException($"Clothe item not found with ID: {id}");

            foreach (PhotoClothes photo in clotheItem.Photos)
            {
                if (!string.IsNullOrEmpty(photo.PhotoURL)) await imageService.DeleteImageAsync(photo.PhotoURL);
            }

            unitOfWork.ClotheItems.Delete(clotheItem);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheService.RemoveAsync($"clothe:{clotheItem.Slug}");
            await clotheItemInvalidationService.InvalidateAllAsync();

            await clotheStockInvalidationService.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();
            await cacheService.RemoveAsync("clothe:top8_most_popular");

            ClotheItemDeletedEvent clotheItemDeletedEvent = new ClotheItemDeletedEvent
            {
                ClotheId = id,
            };
            await publishEndpoint.Publish(clotheItemDeletedEvent, cancellationToken);
        }

        public async Task<List<ClotheSummaryDTO>?> GetTop8MostPopularAsync(CancellationToken cancellationToken = default)
        {
            string cacheKey = "clothe:top8_most_popular";

            return await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    List<ClotheItem> clotheItems = await unitOfWork.ClothePopularity.GetTop8MostPopularAsync(cancellationToken);
                    return mapper.Map<List<ClotheSummaryDTO>>(clotheItems);
                },
                MEMORY_TTL_CLOTHE_PAGE,
                REDIS_TTL_CLOTHE_PAGE
            );
        }
    }
}
