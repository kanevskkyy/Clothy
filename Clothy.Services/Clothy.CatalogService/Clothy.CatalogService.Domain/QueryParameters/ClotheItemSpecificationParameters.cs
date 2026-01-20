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
        public List<string>? Brands { get; set; } = new List<string>();
        public List<string>? Materials { get; set; } = new List<string>();
        public List<string>? Collections { get; set; } = new List<string>();
        public List<string>? Sizes { get; set; } = new List<string>();
        public List<string>? Tags { get; set; } = new List<string>();
        public List<string>? ClothingTypes { get; set; } = new List<string>();
        public List<string>? Colors { get; set; } = new List<string>();
        public bool ShowOnlyWithDiscounts { get; set; } = false;

        public string ToCacheKey()
        {
            StringBuilder stringBuilder = new StringBuilder("clothes:");

            if (!string.IsNullOrEmpty(Name)) stringBuilder.Append($"name:{Name}:");
            if (MinPrice.HasValue) stringBuilder.Append($"minprice:{MinPrice}:");
            if (MaxPrice.HasValue) stringBuilder.Append($"maxprice:{MaxPrice}:");
            if (Brands?.Any() == true) stringBuilder.Append($"brands:{string.Join(",", Brands.OrderBy(x => x))}:");
            if (Collections?.Any() == true) stringBuilder.Append($"collections:{string.Join(",", Collections.OrderBy(x => x))}:");
            if (Sizes?.Any() == true) stringBuilder.Append($"sizes:{string.Join(",", Sizes.OrderBy(x => x))}:");
            if (Tags?.Any() == true) stringBuilder.Append($"tags:{string.Join(",", Tags.OrderBy(x => x))}:");
            if (ClothingTypes?.Any() == true) stringBuilder.Append($"types:{string.Join(",", ClothingTypes.OrderBy(x => x))}:");
            if (Materials?.Any() == true) stringBuilder.Append($"materials:{string.Join(",", Materials.OrderBy(x => x))}:");
            if (Colors?.Any() == true) stringBuilder.Append($"colors:{string.Join(",", Colors.OrderBy(x => x))}:");
            
            stringBuilder.Append($"showOnlyWithDiscounts:{ShowOnlyWithDiscounts}");
            stringBuilder.Append(GetBaseCacheKeyPart());
            return stringBuilder.ToString();
        }
    }
}