using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs;
using Clothy.OrderService.BLL.DTOs.OrderItemDTOs;

namespace Clothy.OrderService.BLL.DTOs.OrderDTOs
{
    public class OrderCreateDTO
    {
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public List<OrderItemCreateDTO> Items { get; set; } = new();
        public DeliveryDetailCreateDTO? DeliveryDetail { get; set; }
    }
}
