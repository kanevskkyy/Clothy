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

        public string ToCacheKey()
        {
            StringBuilder stringBuilder = new StringBuilder("clothes:");

            if (!string.IsNullOrEmpty(Name)) stringBuilder.Append($"name:{Name}:");
            if (MinPrice.HasValue) stringBuilder.Append($"minprice:{MinPrice}:");
            if (MaxPrice.HasValue) stringBuilder.Append($"maxprice:{MaxPrice}:");
            if (BrandIds?.Any() == true) stringBuilder.Append($"brands:{string.Join(",", BrandIds.OrderBy(x => x))}:");
            if (CollectionIds?.Any() == true) stringBuilder.Append($"collections:{string.Join(",", CollectionIds.OrderBy(x => x))}:");
            if (SizeIds?.Any() == true) stringBuilder.Append($"sizes:{string.Join(",", SizeIds.OrderBy(x => x))}:");
            if (TagIds?.Any() == true) stringBuilder.Append($"tags:{string.Join(",", TagIds.OrderBy(x => x))}:");
            if (ClothingTypeIds?.Any() == true) stringBuilder.Append($"types:{string.Join(",", ClothingTypeIds.OrderBy(x => x))}:");

            stringBuilder.Append(GetBaseCacheKeyPart());
            return stringBuilder.ToString();
        }
    }
}