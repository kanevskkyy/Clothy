using Clothy.CatalogService.Domain.Entities.Base;
using Clothy.CatalogService.Domain.Entities.Clothe;

namespace Clothy.CatalogService.Domain.Entities.Catalog
{
    public class Material : BaseEntity
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public ICollection<ClotheMaterial> ClotheMaterials { get; set; } = new List<ClotheMaterial>();
    }
}
