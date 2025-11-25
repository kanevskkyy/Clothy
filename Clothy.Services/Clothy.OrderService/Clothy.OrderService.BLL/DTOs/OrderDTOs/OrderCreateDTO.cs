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
        public Guid PickupPointId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
    }
}
