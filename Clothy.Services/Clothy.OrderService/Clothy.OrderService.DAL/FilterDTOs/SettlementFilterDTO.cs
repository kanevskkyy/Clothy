using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.FilterDTOs
{
    public class SettlementFilterDTO : BaseFilterDTO
    {
        public Guid? RegionId { get; set; }

        public override string ToCacheKey()
        {
            return $"settlements:" +
                   $"region:{RegionId?.ToString() ?? "all"}:" +
                   GetBaseCacheKeyPart();
        }
    }
}
