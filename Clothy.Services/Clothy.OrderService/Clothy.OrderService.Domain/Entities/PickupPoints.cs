using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class PickupPoints : BaseEntity
    {
        public string? Address {  get; set; }
        public Guid DeliveryProviderId { get; set; }
        public Guid SettlementId { get; set; }
        public DeliveryProvider? DeliveryProvider { get; set; }
        public Settlement? Settlement { get; set; }
    }
}
