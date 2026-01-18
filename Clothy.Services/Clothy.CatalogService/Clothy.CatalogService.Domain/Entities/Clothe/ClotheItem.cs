using Clothy.CatalogService.Domain.Entities.Base;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Clothe
{
    public class ClotheItem : BaseEntity
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }

        public Guid? BrandId { get; set; }
        public Brand? Brand { get; set; }

        public Guid? ClothingTypeId { get; set; }
        public ClothingType? ClothyType { get; set; }

        public Guid? CollectionId { get; set; }
        public Collection? Collection { get; set; }

        public ICollection<PhotoClothes> Photos { get; set; } = new List<PhotoClothes>();
        public ICollection<ClothePopularity> ClothePopularities { get; set; } = new List<ClothePopularity>();
        public ICollection<ClothesStock> Stocks { get; set; } = new List<ClothesStock>();
        public ICollection<ClotheTag> ClotheTags { get; set; } = new List<ClotheTag>();
        public ICollection<ClotheMaterial> ClotheMaterials { get; set; } = new List<ClotheMaterial>();
    }
}
