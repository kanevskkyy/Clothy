

using Clothy.Aggregator.Clients;
using Clothy.Aggregator.DTOs.Filters;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.Aggregator.Services
{
    public class FilterAggregatorService
    {
        private CatalogClient catalogClient;
        private ILogger<FilterAggregatorService> logger;

        public FilterAggregatorService(CatalogClient catalogClient, ILogger<FilterAggregatorService> logger)
        {
            this.catalogClient = catalogClient;
            this.logger = logger;
        }

        public async Task<ClotheFiltersDTO?> GetAllFiltersAsync(CancellationToken ct)
        {
            ClotheFiltersDTO filters = new ClotheFiltersDTO();

            filters.Brands = (await catalogClient.GetBrandsAsync(ct)) ?? new List<BrandReadDTO>();
            logger.LogInformation("Brands count: {Count}", filters.Brands?.Count ?? 0);


            filters.ClothingTypes = (await catalogClient.GetClothingTypesAsync(ct)) ?? new List<ClothingTypeReadDTO>();
            filters.Collections = (await catalogClient.GetCollectionsAsync(ct)) ?? new List<CollectionWithCountDTO>();
            filters.Colors = (await catalogClient.GetColorsAsync(ct)) ?? new List<ColorWithCountDTO>();
            filters.Materials = (await catalogClient.GetMaterialsAsync(ct)) ?? new List<MaterialWithCountDTO>();
            filters.Sizes = (await catalogClient.GetSizesAsync(ct)) ?? new List<SizeReadDTO>();
            filters.Tags = (await catalogClient.GetTagsAsync(ct)) ?? new List<TagWithCountDTO>();
            filters.PriceRange = await catalogClient.GetPriceRangeAsync(ct) ?? new PriceRangeDTO();

            logger.LogInformation("Successfully aggregated filters data.");

            return filters;
        }
    }
}
