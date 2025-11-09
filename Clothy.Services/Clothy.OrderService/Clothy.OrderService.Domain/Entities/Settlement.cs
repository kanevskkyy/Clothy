using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class Settlement : BaseEntity
    {
        public string? Name { get; set; }
        public Guid RegionId { get; set; }
        public Region? Region { get; set; }
    }
}
