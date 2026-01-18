using Clothy.CatalogService.Domain.Entities.Base;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Catalog
{
    public class Color : BaseEntity
    {
        public string? Name { get; set; }
        public string? HexCode { get; set; }
     
        public ICollection<ClothesStock> ClothesStocks { get; set; } = new List<ClothesStock>();
        public ICollection<PhotoClothes> PhotoClothes { get; set; } = new List<PhotoClothes>(); 
    }
}
