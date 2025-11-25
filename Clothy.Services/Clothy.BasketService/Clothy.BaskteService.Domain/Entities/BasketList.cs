using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.BaskteService.Domain.Entities
{
    public class BasketList
    {
        public Guid UserId { get; set; }
        public List<BasketItem> BasketItems { get; set; } = new List<BasketItem>();
    }
}
