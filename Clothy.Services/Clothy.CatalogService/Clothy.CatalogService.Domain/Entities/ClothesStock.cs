using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class ClothesStock : BaseEntity
    {
        public Guid ClotheId { get; set; }
        public ClotheItem? Clothe { get; set; }

        public Guid SizeId { get; set; }
        public Size? Size { get; set; }

        public Guid ColorId { get; set; }
        public Color? Color { get; set; }

        public int Quantity { get; set; } = 0;
    }
}
