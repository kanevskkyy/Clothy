using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;

namespace Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs
{
    public class DeliveryDetailDTO
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? DetailsDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
