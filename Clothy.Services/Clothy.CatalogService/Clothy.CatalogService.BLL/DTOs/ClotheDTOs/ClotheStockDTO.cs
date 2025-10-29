using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheStockDTO
    {
        public Guid StockId { get; set; }
        public SizeReadDTO? Size { get; set; } 
        public ColorReadDTO? Color { get; set; }
        public int Quantity { get; set; }
    }
}
