using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs;
using Clothy.OrderService.BLL.DTOs.OrderItemDTOs;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;

namespace Clothy.OrderService.BLL.DTOs.OrderDTOs
{
    public class OrderDetailDTO
    {
        public Guid Id { get; set; }
        public OrderStatusReadDTO? Status { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();
        public DeliveryDetailDTO? DeliveryDetail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
