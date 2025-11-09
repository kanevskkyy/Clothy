using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.RegionDTOs
{
    public class RegionReadDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid CityId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
