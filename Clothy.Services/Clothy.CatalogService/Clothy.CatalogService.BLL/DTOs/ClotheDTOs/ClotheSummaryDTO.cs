using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheSummaryDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? MainPhotoURL { get; set; }
        public decimal Price { get; set; }

        public BrandReadDTO? Brand { get; set; }
        public ClothingTypeReadDTO? ClothyType { get; set; }
        public CollectionReadDTO? Collection { get; set; }

        public int AdditionalPhotosCount { get; set; }
        public int AdditionalColorsCount { get; set; }
        public bool IsAvailable { get; set; }
    }
}
