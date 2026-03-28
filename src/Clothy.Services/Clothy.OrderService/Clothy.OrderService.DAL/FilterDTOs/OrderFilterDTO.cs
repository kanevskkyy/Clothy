using Clothy.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.FilterDTOs
{
    public class OrderFilterDTO : BaseFilterDTO
    {
        public OrderStatus? Status { get; set; }
        public Guid? UserId { get; set; }

        public override string ToCacheKey()
        {
            return $"orders:" +
                   $"status:{Status?.ToString() ?? "all"}:" +
                   $"user:{UserId?.ToString() ?? "all"}:" +
                   GetBaseCacheKeyPart();
        }
    }
}
