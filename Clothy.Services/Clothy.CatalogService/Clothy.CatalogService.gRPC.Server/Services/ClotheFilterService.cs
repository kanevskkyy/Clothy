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
                Dictionary<Brand, int> brands = await unitOfWork.Brands.GetBrandsWithStockCountAsync(context.CancellationToken);
                Dictionary<ClothingType, int> clothingTypes = await unitOfWork.ClothingTypes.GetClothingTypeCountWithStockAsync(context.CancellationToken);
                Dictionary<Collection, int> collections = await unitOfWork.Collections.GetCollectionsCountWithStockAsync(context.CancellationToken);
                Dictionary<Color, int> colors = await unitOfWork.Colors.GetColorsCountWithStockAsync(context.CancellationToken);
                Dictionary<Material, int> materials = await unitOfWork.Materials.GetMaterialsWithStockAsync(context.CancellationToken);
                Dictionary<Tag, int> tags = await unitOfWork.Tags.GetTagsWithStockCountAsync(context.CancellationToken);
                (decimal minPrice, decimal maxPrice) priceRange = await unitOfWork.ClotheItems.GetMinAndMaxPriceAsync(context.CancellationToken);
                (int maleCount, int femaleCount) genderCount = await unitOfWork.ClotheItems.GetClotheItemCountByGenderAsync(context.CancellationToken);

                logger.LogInformation("Succesfully collected all filters!");

                FilterResponse response = new FilterResponse();
                response.Brands.AddRange(ConvertBrandsToGrpcResponse(brands));
                response.ClothingTypes.AddRange(ConvertClothingTypesToGrpcResponse(clothingTypes));
                response.Collections.AddRange(ConvertCollectionsToGrpcResponse(collections));
                response.Colors.AddRange(ConvertColorsToGrpcResponse(colors));
                response.Materials.AddRange(ConvertMaterialsToGrpcResponse(materials));
                response.Tags.AddRange(ConvertTagsToGrpcResponse(tags));
                response.PriceRange = ConvertPriceRangeToGrpcResponse(priceRange);
                response.Gender = ConvertGenderCountToGrpcResponse(genderCount);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while reading...");
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during reading"));
            }
        }

        private List<BrandsGrpcResponse> ConvertBrandsToGrpcResponse(Dictionary<Brand, int> brands)
        {
            return brands.Select(pair => new BrandsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                Slug = pair.Key.Slug,
                ClotheItemCount = pair.Value
            }).ToList();
        }

        private List<ClothingTypesGrpcResponse> ConvertClothingTypesToGrpcResponse(Dictionary<ClothingType, int> clothingTypes)
        {
            return clothingTypes.Select(clothingType => new ClothingTypesGrpcResponse
            {
                Id = clothingType.Key.Id.ToString(),
                Name = clothingType.Key.Name,
                Slug = clothingType.Key.Slug,
                ClotheItemCount = clothingType.Value
            }).ToList();
        }

        private List<CollectionsGrpcResponse> ConvertCollectionsToGrpcResponse(Dictionary<Collection, int> collections)
        {
            return collections.Select(pair => new CollectionsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                Slug = pair.Key.Slug,
                ClotheItemCount = pair.Value
            }).ToList();
        }

        private List<ColorsGrpcResponse> ConvertColorsToGrpcResponse(Dictionary<Color, int> colors)
        {
            return colors.Select(pair => new ColorsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                Slug = pair.Key.Slug,
                ClotheItemCount = pair.Value
            }).ToList();
        }

        private List<MaterialsGrpcResponse> ConvertMaterialsToGrpcResponse(Dictionary<Material, int> materials)
        {
            return materials.Select(pair => new MaterialsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                ClotheItemCount = pair.Value,
                Slug = pair.Key.Slug
            }).ToList();
        }

        private List<TagsGrpcResponse> ConvertTagsToGrpcResponse(Dictionary<Tag, int> tags)
        {
            return tags.Select(pair => new TagsGrpcResponse
            {
                Id = pair.Key.Id.ToString(),
                Name = pair.Key.Name,
                ClotheItemCount = pair.Value,
                Slug = pair.Key.Slug
            }).ToList();
        }

        private PriceGrpcResponse ConvertPriceRangeToGrpcResponse((decimal minPrice, decimal maxPrice) priceRange)
        {
            return new PriceGrpcResponse
            {
                MaxPrice = priceRange.maxPrice.ToString(),
                MinPrice = priceRange.minPrice.ToString(),
            };
        }

        private GenderGrpcResponse ConvertGenderCountToGrpcResponse((int maleCount, int femaleCount) genderCount)
        {
            return new GenderGrpcResponse
            {
                MaleCount = genderCount.maleCount,
                FemaleCount = genderCount.femaleCount,
            };
        }
    }
}
