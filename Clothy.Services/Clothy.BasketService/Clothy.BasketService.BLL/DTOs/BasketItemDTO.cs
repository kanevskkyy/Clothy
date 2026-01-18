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
        public string? ClotheName { get; set; }
        public decimal Price { get; set; }
        public string? MainPhoto { get; set; }
        public string? SizeName { get; set; }
        public string? HexCode { get; set; }
        public string? ColorName { get; set; }
        public int Quantity { get; set; }
    }
}
