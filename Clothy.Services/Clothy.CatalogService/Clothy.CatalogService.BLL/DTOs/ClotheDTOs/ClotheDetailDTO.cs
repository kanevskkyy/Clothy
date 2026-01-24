using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.Domain.Entities.Clothe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheDetailDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 
        public string? Slug { get; set; }
        public Gender Gender { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? DiscountPercent => OldPrice.HasValue
            ? (int)Math.Round((OldPrice.Value - Price) / OldPrice.Value * 100)
            : null;

        public BrandReadDTO? Brand { get; set; }
        public ClothingTypeReadDTO? ClothyType { get; set; }
        public CollectionReadDTO? Collection { get; set; }

        public List<PhotoReadDTO> AdditionalPhotos { get; set; } = new();
        public List<TagReadDTO> Tags { get; set; } = new();
        public List<MaterialWithPercentageDTO> Materials { get; set; } = new();
        public List<ClotheStockDTO> Stocks { get; set; } = new();
    }
}
