using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;

namespace Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs
{
    public class ClothesStockReadDTO
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }

        public SizeReadDTO? Size { get; set; }
        public ColorReadDTO? Color { get; set; }

        public ClotheSummaryDTO? Clothe { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
