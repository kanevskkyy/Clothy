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
        public decimal TotalPrice => Items.Where(i => i.IsAvailable).Sum(basketItem => basketItem.Price * basketItem.Quantity);
        public int TotalItems => Items.Count;
        public int UnavailableItemsCount => Items.Count(i => !i.IsAvailable);
    }
}
