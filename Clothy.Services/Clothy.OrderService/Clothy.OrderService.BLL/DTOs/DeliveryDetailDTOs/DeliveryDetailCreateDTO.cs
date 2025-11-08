using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs
{
    public class DeliveryDetailCreateDTO
    {
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
    }
}
