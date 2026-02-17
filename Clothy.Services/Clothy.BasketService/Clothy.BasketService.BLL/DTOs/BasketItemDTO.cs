using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.BasketService.BLL.DTOs
{
    public class BasketItemDTO
    {
        public Guid ClotheId { get; set; }
        public Guid SizeId { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
        public string ClotheName { get; set; } = string.Empty;
        public string SizeName { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorSlug { get; set; } = string.Empty;
        public string ClotheSlug { get; set; } = string.Empty;
        public string HexCode { get; set; } = string.Empty;
        public string MainPhoto { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;
        public string? ValidationMessage { get; set; }
    }
}
