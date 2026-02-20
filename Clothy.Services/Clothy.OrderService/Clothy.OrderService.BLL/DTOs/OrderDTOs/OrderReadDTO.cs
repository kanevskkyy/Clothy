using Clothy.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.OrderDTOs
{
    public class OrderReadDTO
    {
        public Guid Id { get; set; }
        public OrderStatus? Status { get; set; }
        public Guid UserId { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? UserEmail { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsFreeDelivery => TotalPrice >= 1500;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
