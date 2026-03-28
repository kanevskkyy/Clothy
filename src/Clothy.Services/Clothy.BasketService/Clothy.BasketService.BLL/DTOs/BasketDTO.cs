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
        public decimal OriginalPrice => Items.Where(i => i.IsAvailable).Sum(i => i.Price * i.Quantity);
        public decimal TotalPrice => IsFirstOrder ? OriginalPrice * 0.9m : OriginalPrice;
        public int TotalItems => Items.Where(i => i.IsAvailable).Sum(i => i.Quantity);
        public int UnAvailableItemsCount => Items.Count(i => !i.IsAvailable);
        public bool IsFirstOrder { get; set; }
    }
}