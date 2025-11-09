using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.SettlementDTOs
{
    public class SettlementReadDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid RegionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
