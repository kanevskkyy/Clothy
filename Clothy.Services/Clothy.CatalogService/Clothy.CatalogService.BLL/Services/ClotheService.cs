using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Helpers;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using Clothy.Shared.Exceptions;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.CloudinaryConfig;
using Clothy.Shared.Cache.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Clothy.CatalogService.BLL.Services
{
    public class ClotheService : IClotheService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;
        private IImageService imageService;
        private IEntityCacheService cacheService;
        private IEntityCacheInvalidationService<ClotheItem> cacheInvalidationService;
        private const int MAX_CASHED_PAGES = 10;

        public ClotheService(IUnitOfWork unitOfWork,IMapper mapper, IImageService imageService, IEntityCacheService cacheService, IEntityCacheInvalidationService<ClotheItem> cacheInvalidationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageService = imageService;
            this.cacheService = cacheService;
            this.cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<PagedList<ClotheSummaryDTO>> GetPagedClotheItemsAsync(ClotheItemSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            bool usePageCache = parameters.PageNumber <= MAX_CASHED_PAGES;
            string cacheKey;

            if (usePageCache)
            {
                if (parameters.MinPrice.HasValue && parameters.MaxPrice.HasValue) cacheKey = $"clothes:price:{parameters.MinPrice}-{parameters.MaxPrice}:page:{parameters.PageNumber}:size:{parameters.PageSize}";
                else cacheKey = $"clothes:page:{parameters.PageNumber}:size:{parameters.PageSize}";
            }
            else
            {
                PagedList<ClotheItem> paged = await unitOfWork.ClotheItems.GetPagedClotheItemsAsync(parameters, cancellationToken);
                List<ClotheSummaryDTO> mapped = mapper.Map<List<ClotheSummaryDTO>>(paged.Items);
                return new PagedList<ClotheSummaryDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
            }

            PagedList<ClotheSummaryDTO>? cached = await cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    PagedList<ClotheItem> paged = await unitOfWork.ClotheItems.GetPagedClotheItemsAsync(parameters, cancellationToken);
                    List<ClotheSummaryDTO> mapped = mapper.Map<List<ClotheSummaryDTO>>(paged.Items);
                    return new PagedList<ClotheSummaryDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
                });

            return cached!;
        }


        public async Task<ClotheDetailDTO> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"clothe:{id}";
            var cached = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdWithDetailsAsync(id, cancellationToken);
                if (clotheItem == null) throw new NotFoundException($"Clothe item not found with ID: {id}");
                
                return mapper.Map<ClotheDetailDTO>(clotheItem);
            });

            return cached!;
        }

        public async Task<ClotheDetailDTO> CreateAsync(ClotheCreateDTO dto, CancellationToken cancellationToken = default)
        {
            if (await unitOfWork.ClotheItems.IsSlugAlreadyExistsAsync(dto.Slug, null, cancellationToken)) throw new AlreadyExistsException("Clothe with this slug already exists");

            int totalPercentage = dto.Materials.Sum(p => p.Percentage);
            if (totalPercentage != 100) throw new InvalidMaterialPercentageException("Total material percentage must be exactly 100.");

            ClotheItem clothe = mapper.Map<ClotheItem>(dto);
            clothe.MainPhotoURL = await imageService.UploadAsync(dto.MainPhoto, "clothes");

            clothe.Photos = new List<PhotoClothes>();
            foreach (var photo in dto.AdditionalPhotos)
            {
                string url = await imageService.UploadAsync(photo, "clothes");
                clothe.Photos.Add(new PhotoClothes 
                { 
                    PhotoURL = url 
                });
            }

            if (!await unitOfWork.Materials.AreAllExistAsync(dto.Materials.Select(material => material.MaterialId), cancellationToken)) throw new NotFoundException("One or more materials do not exist.");
            if (!await unitOfWork.Tags.AreAllExistAsync(dto.TagIds, cancellationToken)) throw new NotFoundException("One or more tags do not exist.");

            clothe.ClotheMaterials = dto.Materials
                .Select(clotheMaterial => new ClotheMaterial 
                { 
                    MaterialId = clotheMaterial.MaterialId, 
                    Percentage = clotheMaterial.Percentage 
                })
                .ToList();

            clothe.ClotheTags = dto.TagIds
                .Select(tagId => new ClotheTag { 
                    TagId = tagId 
                })
                .ToList();

            await unitOfWork.ClotheItems.AddAsync(clothe, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheInvalidationService.InvalidateAllAsync();

            return await GetDetailByIdAsync(clothe.Id, cancellationToken);
        }

        public async Task<ClotheDetailDTO> UpdateAsync(Guid id, ClotheUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            ClotheItem? clotheItem = await unitOfWork.ClotheItems.GetByIdWithDetailsAsync(id, cancellationToken);
            if (clotheItem == null) throw new NotFoundException($"Clothe item not found with ID: {id}");

            if (await unitOfWork.ClotheItems.IsSlugAlreadyExistsAsync(dto.Slug, id, cancellationToken)) throw new AlreadyExistsException("Clothe with this slug already exists");

            int totalPercentage = dto.Materials.Sum(percentage => percentage.Percentage);
            if (totalPercentage != 100) throw new InvalidMaterialPercentageException("Total material percentage must be exactly 100.");

            mapper.Map(dto, clotheItem);

            if (dto.MainPhoto != null)
            {
                if (!string.IsNullOrEmpty(clotheItem.MainPhotoURL)) await imageService.DeleteImageAsync(clotheItem.MainPhotoURL);
                clotheItem.MainPhotoURL = await imageService.UploadAsync(dto.MainPhoto, "clothes");
            }

            if (dto.AdditionalPhotos?.Any() == true)
            {
                foreach (IFormFile photo in dto.AdditionalPhotos)
                {
                    string url = await imageService.UploadAsync(photo, "clothes");
                    clotheItem.Photos.Add(new PhotoClothes { 
                        PhotoURL = url 
                    });
                }
            }

            if (!await unitOfWork.Materials.AreAllExistAsync(dto.Materials.Select(material => material.MaterialId), cancellationToken)) throw new NotFoundException("One or more materials do not exist.");
            if (!await unitOfWork.Tags.AreAllExistAsync(dto.TagIds, cancellationToken)) throw new NotFoundException("One or more tags do not exist.");

            clotheItem.ClotheMaterials.Clear();
            foreach (ClotheMaterialCreateDTO material in dto.Materials)
            {
                clotheItem.ClotheMaterials.Add(new ClotheMaterial { 
                    MaterialId = material.MaterialId, 
                    Percentage = material.Percentage 
                });
            }

            clotheItem.ClotheTags.Clear();
            foreach (Guid tagId in dto.TagIds)
            {
                clotheItem.ClotheTags.Add(new ClotheTag { 
                    TagId = tagId 
                });
            }

            unitOfWork.ClotheItems.Update(clotheItem);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await cacheInvalidationService.InvalidateByIdAsync(id);

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

            if (!string.IsNullOrEmpty(clothe.MainPhotoURL)) await imageService.DeleteImageAsync(clothe.MainPhotoURL);

            foreach (PhotoClothes photo in clothe.Photos)
            {
                if (!string.IsNullOrEmpty(photo.PhotoURL)) await imageService.DeleteImageAsync(photo.PhotoURL);
            }

            unitOfWork.ClotheItems.Delete(clothe);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheInvalidationService.InvalidateByIdAsync(id);
        }
    }
}
