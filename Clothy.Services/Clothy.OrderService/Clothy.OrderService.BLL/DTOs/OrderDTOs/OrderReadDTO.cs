using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;

namespace Clothy.OrderService.BLL.DTOs.OrderDTOs
{
    public class OrderReadDTO
    {
        public Guid Id { get; set; }
        public OrderStatusReadDTO? Status { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public double TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
