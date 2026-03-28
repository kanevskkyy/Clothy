using Clothy.CatalogService.Domain.Entities.Clothe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.QueryParameters
{
    public class ClotheItemSpecificationParameters : BaseSpecificationParameters
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }                         
        public Gender? Gender { get; set; }                            
        public List<Guid>? Brands { get; set; } = new List<Guid>();    
        public List<Guid>? Materials { get; set; } = new List<Guid>();
        public List<Guid>? Collections { get; set; } = new List<Guid>();
        public List<Guid>? Tags { get; set; } = new List<Guid>();
        public List<Guid>? ClothingTypes { get; set; } = new List<Guid>();
        public List<Guid>? Colors { get; set; } = new List<Guid>();

        public string ToCacheKey()
        {
            StringBuilder stringBuilder = new StringBuilder("clothes:");

            if (MinPrice.HasValue) stringBuilder.Append($"minprice:{MinPrice}:");
            if (MaxPrice.HasValue) stringBuilder.Append($"maxprice:{MaxPrice}:");
            if (Gender.HasValue) stringBuilder.Append($"gender:{Gender.Value}:");
            if (Brands?.Any() == true) stringBuilder.Append($"brands:{string.Join(",", Brands.OrderBy(x => x))}:");
            if (Collections?.Any() == true) stringBuilder.Append($"collections:{string.Join(",", Collections.OrderBy(x => x))}:");
            if (Tags?.Any() == true) stringBuilder.Append($"tags:{string.Join(",", Tags.OrderBy(x => x))}:");
            if (ClothingTypes?.Any() == true) stringBuilder.Append($"types:{string.Join(",", ClothingTypes.OrderBy(x => x))}:");
            if (Materials?.Any() == true) stringBuilder.Append($"materials:{string.Join(",", Materials.OrderBy(x => x))}:");
            if (Colors?.Any() == true) stringBuilder.Append($"colors:{string.Join(",", Colors.OrderBy(x => x))}:");
            
            stringBuilder.Append(GetBaseCacheKeyPart());
            return stringBuilder.ToString();
        }
    }
}