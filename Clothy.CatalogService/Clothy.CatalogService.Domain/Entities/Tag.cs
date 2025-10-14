using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string? Name { get; set; }
        public ICollection<ClotheTag> ClotheTags { get; set; } = new List<ClotheTag>();
    }
}
