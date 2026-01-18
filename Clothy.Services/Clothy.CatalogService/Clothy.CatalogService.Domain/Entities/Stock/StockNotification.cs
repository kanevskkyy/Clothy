using Clothy.CatalogService.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Stock
{
    public class StockNotification : BaseEntity
    {
        public Guid StockId { get; set; }
        public ClothesStock? Stock { get; set; }

        public Guid UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserFirstName { get; set; }

        public bool IsNotified { get; set; } = false;
    }
}
