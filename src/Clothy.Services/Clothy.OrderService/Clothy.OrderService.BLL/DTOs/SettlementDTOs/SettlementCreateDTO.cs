using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.SettlementDTOs
{
    public class SettlementCreateDTO
    {
        public SettlementType Type { get; set; }
        public string? Ref { get; set; }
        public string? Name { get; set; }
        public Guid RegionId { get; set; }
    }
}
