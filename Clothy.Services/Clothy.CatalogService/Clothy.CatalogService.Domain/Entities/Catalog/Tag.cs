using Clothy.CatalogService.Domain.Entities.Base;
using Clothy.CatalogService.Domain.Entities.Clothe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Catalog
{
    public class Tag : BaseEntity
    {
        public string? Name { get; set; }
        public ICollection<ClotheTag> ClotheTags { get; set; } = new List<ClotheTag>();
    }
}
