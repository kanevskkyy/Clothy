using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class ClothingType : BaseEntity
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }

        public ICollection<ClotheItem> Items { get; set; } = new List<ClotheItem>();
    }
}
