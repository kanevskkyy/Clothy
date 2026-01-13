using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.Helpers.ModelBinder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheUpdateDTO
    {
        public string? Name { get; set; } 
        public string? Slug { get; set; } 
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public Guid BrandId { get; set; }
        public Guid ClothingTypeId { get; set; }
        public Guid CollectionId { get; set; }
    }
}
