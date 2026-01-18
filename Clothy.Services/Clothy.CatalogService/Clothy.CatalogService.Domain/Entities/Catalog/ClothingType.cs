using Clothy.CatalogService.Domain.Entities.Base;
using Clothy.CatalogService.Domain.Entities.Clothe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Catalog
{
    public class ClothingType : BaseEntity
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }

        public ICollection<ClotheItem> Items { get; set; } = new List<ClotheItem>();
    }
}
