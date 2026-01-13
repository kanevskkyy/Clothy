using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Helpers;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
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

namespace Clothy.CatalogService.BLL.Services
{
    public class ClotheService : IClotheService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<ClotheItem> cacheInvalidationService;
        private IPublishEndpoint publishEndpoint;
        private ILogger<ClotheService> logger;
        private IEntityCacheInvalidationService<ClothesStock> cacheInvalidatonClotheStock;
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
            Meter meter, ILogger<ClotheService> logger, 
            IEntityCacheInvalidationService<ClothesStock> cacheInvalidatonClotheStock, 
            IPublishEndpoint publishEndpoint,
            IFilterCacheInvalidationService filterCacheInvalidationService)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
            clotheCreatedMetric = meter.CreateCounter<long>(
                "clothy.catalog.clotheItem.created_total",
                "items",
                "Total numbers of clothes created");
            this.cacheInvalidatonClotheStock = cacheInvalidatonClotheStock;
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

        public async Task<ClotheDetailDTO> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"clothe:{id}";
            ClotheDetailDTO? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdWithDetailsAsync(id, cancellationToken);
                    if (clotheItem == null) throw new NotFoundException($"Clothe item not found with ID: {id}");
                    return mapper.Map<ClotheDetailDTO>(clotheItem);
                },
                MEMORY_TTL_CLOTHE_DETAIL,
                REDIS_TTL_CLOTHE_DETAIL
            );

            return cached!;
        }

        public async Task<ClotheDetailDTO> CreateAsync(ClotheCreateDTO dto, CancellationToken cancellationToken = default)
        {
            if (await unitOfWork.ClotheItems.IsSlugAlreadyExistsAsync(dto.Slug, null, cancellationToken)) throw new AlreadyExistsException("Clothe with this slug already exists");

            List<ClotheMaterialCreateDTO> distinctMaterials = dto.Materials
                .GroupBy(m => m.MaterialId)
                .Select(g => g.First())
                .ToList();

            int totalPercentage = distinctMaterials.Sum(p => p.Percentage);
            if (totalPercentage != 100) throw new InvalidMaterialPercentageException("Total material percentage must be exactly 100.");

            Brand? brand = await unitOfWork.Brands.GetByIdAsync(dto.BrandId, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand with ID: {dto.BrandId} not found");

            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(dto.ClothingTypeId, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"Clothing type with ID: {dto.ClothingTypeId} not found");

            Collection? collection = await unitOfWork.Collections.GetByIdAsync(dto.CollectionId, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection with ID: {dto.CollectionId} not found");

            ClotheItem clothe = mapper.Map<ClotheItem>(dto);

            bool isDuplicated = dto.AdditionalPhotos
                .Where(p => p.IsMain)
                .GroupBy(p => p.ColorId)
                .Any(g => g.Count() > 1);

            if (isDuplicated) throw new ValidationFailedException("Only one main photo per color is allowed.");

            clothe.Photos = new List<PhotoClothes>();
            
            var colorGroups = dto.AdditionalPhotos
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

            foreach (ClothePhotoCreateDTO clothePhotoCreateDTO in dto.AdditionalPhotos)
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
            if (!await unitOfWork.Tags.AreAllExistAsync(dto.TagIds, cancellationToken)) throw new NotFoundException("One or more tags do not exist.");

            List<Guid> distinctTags = dto.TagIds.Distinct().ToList();

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

            await cacheInvalidationService.InvalidateAllAsync();
            await cacheInvalidatonClotheStock.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();

            return await GetDetailByIdAsync(clothe.Id, cancellationToken);
        }

        public async Task<ClotheDetailDTO> UpdateAsync(Guid id, ClotheUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdAsync(id, cancellationToken);
            if (clotheItem == null) throw new NotFoundException($"Clothe item not found with ID: {id}");

            if (await unitOfWork.ClotheItems.IsSlugAlreadyExistsAsync(dto.Slug, id, cancellationToken)) throw new AlreadyExistsException("Clothe with this slug already exists");

            Brand? brand = await unitOfWork.Brands.GetByIdAsync(dto.BrandId, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand with ID: {dto.BrandId} not found");

            ClothingType? clothingType = await unitOfWork.ClothingTypes.GetByIdAsync(dto.ClothingTypeId, cancellationToken);
            if (clothingType == null) throw new NotFoundException($"Clothing type with ID: {dto.ClothingTypeId} not found");

            Collection? collection = await unitOfWork.Collections.GetByIdAsync(dto.CollectionId, cancellationToken);
            if (collection == null) throw new NotFoundException($"Collection with ID: {dto.CollectionId} not found");

            mapper.Map(dto, clotheItem);

            unitOfWork.ClotheItems.Update(clotheItem);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheInvalidationService.InvalidateAllAsync();
            await cacheInvalidationService.InvalidateByIdAsync(id);

            ClotheItemUpdatedEvent clotheItemUpdatedEvent = new ClotheItemUpdatedEvent
            {
                ClotheId = clotheItem.Id,
                ClotheName = clotheItem.Name,
                Price = clotheItem.Price,
            };
            await publishEndpoint.Publish(clotheItemUpdatedEvent, cancellationToken);
            await cacheInvalidatonClotheStock.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();

            return await GetDetailByIdAsync(clotheItem.Id, cancellationToken);
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
            ClotheItem? clothe = await unitOfWork.ClotheItems.GetByIdWithDetailsAsync(id, cancellationToken);
            if (clothe == null) throw new NotFoundException($"Clothe item not found with ID: {id}");

            foreach (PhotoClothes photo in clothe.Photos)
            {
                if (!string.IsNullOrEmpty(photo.PhotoURL)) await imageService.DeleteImageAsync(photo.PhotoURL);
            }

            unitOfWork.ClotheItems.Delete(clothe);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheInvalidationService.InvalidateByIdAsync(id);
            await cacheInvalidationService.InvalidateAllAsync();

            await cacheInvalidatonClotheStock.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();

            ClotheItemDeletedEvent clotheItemDeletedEvent = new ClotheItemDeletedEvent
            {
                ClotheId = id,
            };
            await publishEndpoint.Publish(clotheItemDeletedEvent, cancellationToken);
        }
    }
}
