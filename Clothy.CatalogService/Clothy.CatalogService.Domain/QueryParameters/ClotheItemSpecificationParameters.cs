using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.QueryParameters
{
    public class ClotheItemSpecificationParameters : BaseSpecificationParameters
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public List<Guid>? BrandIds { get; set; } = new List<Guid>();
        public List<Guid>? CollectionIds { get; set; } = new List<Guid>();
        public List<Guid>? SizeIds { get; set; } = new List<Guid>();
        public List<Guid>? TagIds { get; set; } = new List<Guid>();
        public List<Guid>? ClothingTypeIds { get; set; } = new List<Guid>();
    }
}
