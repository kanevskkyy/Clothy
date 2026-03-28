using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.QueryParameters
{
    public class ClothesStockSpecificationParameters : BaseSpecificationParameters
    {
        public string? Name { get; set; }
        public Guid? ClotheId { get; set; }
        public Guid? SizeId { get; set; }
        public Guid? ColorId { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }

        public string ToCacheKey()
        {
            StringBuilder stringBuilder = new StringBuilder("clothesstock:");

            if (!string.IsNullOrEmpty(Name)) stringBuilder.Append($"name:{Name}:");
            if (ClotheId.HasValue) stringBuilder.Append($"clothe:{ClotheId}:");
            if (SizeId.HasValue) stringBuilder.Append($"size:{SizeId}:");
            if (ColorId.HasValue) stringBuilder.Append($"color:{ColorId}:");
            if (MinQuantity.HasValue) stringBuilder.Append($"minqty:{MinQuantity}:");
            if (MaxQuantity.HasValue) stringBuilder.Append($"maxqty:{MaxQuantity}:");

            stringBuilder.Append(GetBaseCacheKeyPart());
            return stringBuilder.ToString();
        }
    }
}