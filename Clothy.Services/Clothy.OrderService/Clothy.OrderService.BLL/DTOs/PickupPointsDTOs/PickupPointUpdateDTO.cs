using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.PickupPointsDTOs
{
    public class PickupPointUpdateDTO
    {
        public string? Address { get; set; }
        public Guid DeliveryProviderId { get; set; }
    }
}
