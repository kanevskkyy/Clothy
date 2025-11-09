using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.SettlementDTOs
{
    public class SettlementCreateDTO
    {
        public string? Name { get; set; }
        public Guid RegionId { get; set; }
    }
}
