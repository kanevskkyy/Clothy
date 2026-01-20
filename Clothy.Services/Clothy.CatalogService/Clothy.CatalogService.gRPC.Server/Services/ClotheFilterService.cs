using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.CatalogService.gRPC.Server.Services
{
    public class ClotheFilterService : ClotheFilterServiceGrpc.ClotheFilterServiceGrpcBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<ClotheFilterService> logger;

        public ClotheFilterService(IUnitOfWork unitOfWork, ILogger<ClotheFilterService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<FilterResponse> GetAllFilters(Empty request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching all filters...");

            try
            {
                IReadOnlyList<Brand> brands = await unitOfWork.Brands.GetAllAsync(context.CancellationToken);
                IReadOnlyList<ClothingType> clothingTypes = await unitOfWork.ClothingTypes.GetAllAsync(context.CancellationToken);
                Dictionary<Collection, int> collections = await unitOfWork.Collections.GetCollectionsCountWithStockAsync(context.CancellationToken);
                Dictionary<Color, int> colors = await unitOfWork.Colors.GetColorsCountWithStockAsync(context.CancellationToken);
                Dictionary<Material, int> materials = await unitOfWork.Materials.GetMaterialsWithStockAsync(context.CancellationToken);
                IReadOnlyList<Size> sizes = await unitOfWork.Sizes.GetAllAsync(context.CancellationToken);
                Dictionary<Tag, int> tags = await unitOfWork.Tags.GetTagsWithStockCountAsync(context.CancellationToken);
                (decimal minPrice, decimal maxPrice) priceRange = await unitOfWork.ClotheItems.GetMinAndMaxPriceAsync(context.CancellationToken);

                logger.LogInformation("Succesfully collected all filters!");

                FilterResponse response = new FilterResponse();
                response.Brands.AddRange(convertBrandsToGrpcResponse(brands));
                response.ClothingTypes.AddRange(convertClothingTypesToGrpcResponse(clothingTypes));
                response.Collections.AddRange(convertCollectionsToGrpcResponse(collections));
                response.Colors.AddRange(convertColorsToGrpcResponse(colors));
                response.Materials.AddRange(convertMaterialsToGrpcResponse(materials));
                response.Sizes.AddRange(convertSizesToGrpcResponse(sizes));
                response.Tags.AddRange(convertTagsToGrpcResponse(tags));
                response.PriceRange = convertPriceRangeToGrpcResponse(priceRange);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while reading...");
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during reading"));
            }
        }

        private List<BrandsGrpcResponse> convertBrandsToGrpcResponse(IReadOnlyList<Brand> brands)
        {
            return brands.Select(brand => new BrandsGrpcResponse
            {
                Id = brand.Id.ToString(),
                Name = brand.Name,
                Slug = brand.Slug,
                PhotoUrl = brand.PhotoURL,
            }).ToList();
        }

        private List<ClothingTypesGrpcResponse> convertClothingTypesToGrpcResponse(IReadOnlyList<ClothingType> clothingTypes)
        {
            return clothingTypes.Select(clothingType => new ClothingTypesGrpcResponse
            {
                Id = clothingType.Id.ToString(),
                Name = clothingType.Name,
                Slug = clothingType.Slug,
            }).ToList();
        }

        private List<CollectionsGrpcResponse> convertCollectionsToGrpcResponse(Dictionary<Collection, int> collections)
        {
            return collections.Select(pair => new CollectionsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                Slug = pair.Key.Slug,
                ClotheItemCount = pair.Value
            }).ToList();
        }

        private List<ColorsGrpcResponse> convertColorsToGrpcResponse(Dictionary<Color, int> colors)
        {
            return colors.Select(pair => new ColorsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                HexCode = pair.Key.HexCode,
                Slug = pair.Key.Slug,
                ClotheItemCount = pair.Value
            }).ToList();
        }

        private List<MaterialsGrpcResponse> convertMaterialsToGrpcResponse(Dictionary<Material, int> materials)
        {
            return materials.Select(pair => new MaterialsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                ClotheItemCount = pair.Value,
                Slug = pair.Key.Slug
            }).ToList();
        }

        private List<SizesGrpcResponse> convertSizesToGrpcResponse(IReadOnlyList<Size> sizes)
        {
            return sizes.Select(size => new SizesGrpcResponse
            {
                Id = size.Id.ToString(),
                Name = size.Name,
                Slug = size.Slug
            }).ToList();
        }   

        private List<TagsGrpcResponse> convertTagsToGrpcResponse(Dictionary<Tag, int> tags)
        {
            return tags.Select(pair => new TagsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                ClotheItemCount = pair.Value,
                Slug = pair.Key.Slug
            }).ToList();
        }

        private PriceGrpcResponse convertPriceRangeToGrpcResponse((decimal minPrice, decimal maxPrice) priceRange)
        {
            return new PriceGrpcResponse
            {
                MaxPrice = priceRange.maxPrice.ToString(),
                MinPrice = priceRange.minPrice.ToString(),
            };
        }
    }
}
