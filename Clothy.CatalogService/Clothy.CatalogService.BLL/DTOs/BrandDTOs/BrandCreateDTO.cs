using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.CatalogService.BLL.DTOs.BrandDTOs
{
    public class BrandCreateDTO
    {
        public string? Name { get; set; } 
        public string? Slug { get; set; } 
        public IFormFile? Photo { get; set; }
    }
}
