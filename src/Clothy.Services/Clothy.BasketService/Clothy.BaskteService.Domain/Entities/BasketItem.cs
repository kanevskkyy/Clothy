using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.BaskteService.Domain.Entities
{
    public class BasketItem
    {
        public Guid ClotheId { get; set; }
        public Guid SizeId { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
    }
}
