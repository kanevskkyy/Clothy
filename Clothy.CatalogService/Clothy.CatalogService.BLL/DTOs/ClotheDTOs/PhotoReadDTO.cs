using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class PhotoReadDTO
    {
        public Guid Id { get; set; }
        public string? PhotoURL { get; set; } 
    }
}
