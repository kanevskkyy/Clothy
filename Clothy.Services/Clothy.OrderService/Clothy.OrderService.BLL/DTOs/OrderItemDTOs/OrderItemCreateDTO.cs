using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.OrderService.BLL.DTOs.OrderItemDTOs
{
    public class OrderItemCreateDTO
    {
        public Guid ClotheId { get; set; }
        public string? ClotheName { get; set; }      
        public double Price { get; set; }
        public string? MainPhoto { get; set; }
        public Guid ColorId { get; set; }
        public string? HexCode { get; set; }        
        public Guid SizeId { get; set; }
        public string? SizeName { get; set; }       
        public int Quantity { get; set; }
    }
}
