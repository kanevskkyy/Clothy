using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.BasketService.BLL.DTOs
{
    public class BasketItemCreateDTO
    {
        public Guid ClotheId { get; set; }
        public Guid SizeId { get; set; }
        public Guid ColorId { get; set; }
        public int Quantity { get; set; }
    }
}
