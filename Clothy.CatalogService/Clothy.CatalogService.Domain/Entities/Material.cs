using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class Material : BaseEntity
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public ICollection<ClotheMaterial> ClotheMaterials { get; set; } = new List<ClotheMaterial>();
    }
}
