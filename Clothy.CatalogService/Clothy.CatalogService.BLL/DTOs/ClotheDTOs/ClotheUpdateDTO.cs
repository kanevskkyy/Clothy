using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheUpdateDTO
    {
        public string? Name { get; set; } 
        public string? Slug { get; set; } 
        public string? Description { get; set; }
        public IFormFile? MainPhoto { get; set; }
        public decimal Price { get; set; }

        public Guid BrandId { get; set; }
        public Guid ClothingTypeId { get; set; }
        public Guid CollectionId { get; set; }

        public List<IFormFile>? AdditionalPhotos { get; set; }
        public List<Guid>? TagIds { get; set; }
        public List<ClotheMaterialCreateDTO>? Materials { get; set; }
    }
}
