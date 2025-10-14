using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class Color : BaseEntity
    {
        public string? HexCode { get; set; }
        public ICollection<ClothesStock> ClothesStocks { get; set; } = new List<ClothesStock>();
    }
}
