using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public string? Name { get; set; }
        public string? PhotoURL { get; set; }

        public ICollection<ClotheItem> ClotheItems { get; set; } = new List<ClotheItem>();
    }
}
