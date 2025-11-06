using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.DTOs.Filters
{
    public class ClotheFiltersDTO
    {
        public List<BrandsGrpcResponse> Brands { get; set; } = new();
        public List<ClothingTypesGrpcResponse> ClothingTypes { get; set; } = new();
        public List<CollectionsGrpcResponse> Collections { get; set; } = new();
        public List<ColorsGrpcResponse> Colors { get; set; } = new();
        public List<MaterialsGrpcResponse> Materials { get; set; } = new();
        public List<SizesGrpcResponse> Sizes { get; set; } = new();
        public List<TagsGrpcResponse> Tags { get; set; } = new();
        public PriceGrpcResponse PriceRange { get; set; } = new();
    }
}
