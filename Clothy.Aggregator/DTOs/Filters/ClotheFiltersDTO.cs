using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;

namespace Clothy.Aggregator.DTOs.Filters
{
    public class ClotheFiltersDTO
    {
        public List<BrandReadDTO> Brands { get; set; } = new();
        public List<ClothingTypeReadDTO> ClothingTypes { get; set; } = new();
        public List<CollectionWithCountDTO> Collections { get; set; } = new();
        public List<ColorWithCountDTO> Colors { get; set; } = new();
        public List<MaterialWithCountDTO> Materials { get; set; } = new();
        public List<SizeReadDTO> Sizes { get; set; } = new();
        public List<TagWithCountDTO> Tags { get; set; } = new();
        public PriceRangeDTO PriceRange { get; set; } = new();
    }
}
