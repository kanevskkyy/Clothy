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
    }
}
