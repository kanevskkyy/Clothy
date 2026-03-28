using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.FilterDTOs
{
    public class PickupPointFilterDTO : BaseFilterDTO
    {
        public Guid? DeliveryProviderId { get; set; }
        public Guid? SettlementId { get; set; }
        public string? Address { get; set; }

        public override string ToCacheKey()
        {
            return $"pickuppoints:" +
                   $"provider:{DeliveryProviderId?.ToString() ?? "all"}:" +
                   $"settlement:{SettlementId?.ToString() ?? "all"}:" +
                   $"address: {Address}" +
                   GetBaseCacheKeyPart();
        }
    }
}