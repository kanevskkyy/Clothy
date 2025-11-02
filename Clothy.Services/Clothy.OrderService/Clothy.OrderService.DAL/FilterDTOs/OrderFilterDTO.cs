using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.FilterDTOs
{
    public class OrderFilterDTO : BaseFilterDTO
    {
        public Guid? StatusId { get; set; }
        public Guid? UserId { get; set; }
    }
}
