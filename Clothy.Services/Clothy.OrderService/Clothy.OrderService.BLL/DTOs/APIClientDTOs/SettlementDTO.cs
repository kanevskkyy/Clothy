using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.APIClientDTOs
{
    public class SettlementDTO
    {
        public string? Description { get; set; }
        public string? Ref { get; set; }
        public string? SettlementTypeDescription { get; set; }
    }
}
