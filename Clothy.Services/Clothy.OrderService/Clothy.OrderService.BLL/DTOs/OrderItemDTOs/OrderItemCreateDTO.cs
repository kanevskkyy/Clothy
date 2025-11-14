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
        public Guid ColorId { get; set; }
        public Guid SizeId { get; set; }
        public int Quantity { get; set; }
    }
}
