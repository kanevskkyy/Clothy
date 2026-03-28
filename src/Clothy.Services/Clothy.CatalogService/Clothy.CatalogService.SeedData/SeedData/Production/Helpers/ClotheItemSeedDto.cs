using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.SeedData.SeedData.Production.Helpers
{
    record ClotheItemSeedDto(
        string Name, string Slug, string Description,
        decimal Price, decimal? OldPrice,
        string Brand, string Type, string Collection,
        List<string> Tags,
        List<MaterialSeedDto> Materials,
        List<PhotoSeedDto> Photos);
}