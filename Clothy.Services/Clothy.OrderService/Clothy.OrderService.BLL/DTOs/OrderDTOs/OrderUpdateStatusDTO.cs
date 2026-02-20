using Clothy.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.OrderDTOs
{
    public class OrderUpdateStatusDTO
    {
        public OrderStatus Status { get; set; }
    }
}