using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.RegionDTOs
{
    public class RegionUpdateDTO
    {
        public string? Name { get; set; }
        public Guid CityId { get; set; }
    }
}
