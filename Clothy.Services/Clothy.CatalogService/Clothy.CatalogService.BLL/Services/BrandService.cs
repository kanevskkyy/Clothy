using AutoMapper;
using Clothy.Aggregator.Aggregate.RedisCache;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Helpers;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.Shared.Cache.Interfaces;
using Clothy.Shared.Helpers.CloudinaryConfig.ImageService;
using Clothy.Shared.Helpers.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.Services
{
    public class BrandService : IBrandService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;
        private IFilterCacheInvalidationService filterCacheInvalidationService;
        private IEntityCacheInvalidationService<ClotheItem> clotheItemInvalidationService;
        private IEntityCacheInvalidationService<Brand> brandInvalidationService;
        private IEntityCacheService cacheService;
        private Counter<long> brandsCreatedCounter;

        private static TimeSpan MEMORY_TTL_BRANDS = TimeSpan.FromMinutes(30);
        private static TimeSpan REDIS_TTL_BRANDS = TimeSpan.FromHours(1);

        public BrandService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IImageService imageService,
            IFilterCacheInvalidationService filterCacheInvalidationService,
            Meter meter,
            IEntityCacheInvalidationService<ClotheItem> clotheItemInvalidationService,
            IEntityCacheInvalidationService<Brand> brandInvalidationService,
            IEntityCacheService cacheService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.filterCacheInvalidationService = filterCacheInvalidationService;
            this.imageService = imageService;
            this.clotheItemInvalidationService = clotheItemInvalidationService;
            this.brandInvalidationService = brandInvalidationService;
            this.cacheService = cacheService;

            brandsCreatedCounter = meter.CreateCounter<long>(
                "clothy.catalog.brands.createdBrands",
                "items",
                "Number of brands created"
            );
        }

        public async Task<List<BrandReadDTO>?> GetAllAsync(CancellationToken cancellationToken = default)
        {
            string cacheKey = "brands:all";

            return await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var brands = await unitOfWork.Brands.GetAllAsync(cancellationToken);
                    return mapper.Map<List<BrandReadDTO>>(brands);
                },
                MEMORY_TTL_BRANDS,
                REDIS_TTL_BRANDS
            );
        }

        public async Task<BrandReadDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"brand:{id}";

            return await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    Brand? brand = await unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
                    if (brand == null) throw new NotFoundException($"Brand not found with ID: {id}");
                    return mapper.Map<BrandReadDTO>(brand);
                },
                MEMORY_TTL_BRANDS,
                REDIS_TTL_BRANDS
            );
        }

        public async Task<BrandReadDTO> CreateAsync(BrandCreateDTO brandCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Brands.IsNameAlreadyExistsAsync(brandCreateDTO.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this name already exists");

            exists = await unitOfWork.Brands.IsSlugAlreadyExistsAsync(brandCreateDTO.Slug, null, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this slug already exists");

            string photoUrl = await imageService.UploadAsync(brandCreateDTO.Photo, "brands", true);

            Brand brand = mapper.Map<Brand>(brandCreateDTO);
            brand.PhotoURL = photoUrl;

            await unitOfWork.Brands.AddAsync(brand, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            brandsCreatedCounter.Add(1, new KeyValuePair<string, object?>("userType", "Admin"));

            await brandInvalidationService.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();

            return mapper.Map<BrandReadDTO>(brand);
        }

        public async Task<BrandReadDTO> UpdateAsync(Guid id, BrandUpdateDTO brandUpdateDTO, CancellationToken cancellationToken = default)
        {
            Brand? brand = await unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand not found with ID: {id}");

            bool exists = await unitOfWork.Brands.IsNameAlreadyExistsAsync(brandUpdateDTO.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this name already exists");

            exists = await unitOfWork.Brands.IsSlugAlreadyExistsAsync(brandUpdateDTO.Slug, id, cancellationToken);
            if (exists) throw new AlreadyExistsException("Brand with this slug already exists");

            if (brandUpdateDTO.Photo != null)
            {
                if (!string.IsNullOrEmpty(brand.PhotoURL)) await imageService.DeleteImageAsync(brand.PhotoURL);
                brand.PhotoURL = await imageService.UploadAsync(brandUpdateDTO.Photo, "brands");
            }

            mapper.Map(brandUpdateDTO, brand);
            unitOfWork.Brands.Update(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await brandInvalidationService.InvalidateAllAsync();
            await brandInvalidationService.InvalidateByIdAsync(id);

            await filterCacheInvalidationService.InvalidateAsync();
            await clotheItemInvalidationService.InvalidateAllAsync();

            return mapper.Map<BrandReadDTO>(brand);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Brand? brand = await unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
            if (brand == null) throw new NotFoundException($"Brand not found with ID: {id}");

            if (!string.IsNullOrEmpty(brand.PhotoURL)) await imageService.DeleteImageAsync(brand.PhotoURL);

            unitOfWork.Brands.Delete(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await brandInvalidationService.InvalidateByIdAsync(id);
            await brandInvalidationService.InvalidateAllAsync();
            await filterCacheInvalidationService.InvalidateAsync();
            await clotheItemInvalidationService.InvalidateAllAsync();
        }
    }
}