using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;

namespace Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs
{
    public class DeliveryDetailDTO
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public PickupPointReadDTO? PickupPoint { get; set; }
        public DeliveryProviderReadDTO? DeliveryProvider { get; set; }
        public SettlementReadDTO? Settlement { get; set; }
        public RegionReadDTO? Region { get; set; }
        public CityReadDTO? City { get; set; }
    }
}
