using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.OrderService.BLL.DTOs.OrderStatusDTOs
{
    public class OrderStatusCreateDTO
    {
        public string Name { get; set; }
        public IFormFile Icon { get; set; }
    }
}
