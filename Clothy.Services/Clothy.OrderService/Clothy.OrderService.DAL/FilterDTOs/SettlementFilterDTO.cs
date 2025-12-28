using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clothy.OrderService.DAL.FilterDTOs
{
    public class SettlementFilterDTO : BaseFilterDTO
    {
        public Guid? RegionId { get; set; }
        public string? Name { get; set; }

        public override string ToCacheKey()
        {
            return $"settlements:" +
                   $"region:{RegionId?.ToString() ?? "all"}:" +
                   $"name: {Name}" +
                   GetBaseCacheKeyPart();
        }
    }
}
