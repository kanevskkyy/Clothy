using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.BasketService.BLL.DTOs
{
    public class BasketDTO
    {
        public Guid UserId { get; set; }
        public List<BasketItemDTO> Items { get; set; } = new List<BasketItemDTO>();
        public decimal TotalPrice => Items.Sum(basketItem => basketItem.Price * basketItem.Quantity);
    }
}
