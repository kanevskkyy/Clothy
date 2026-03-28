using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.PhotoDTOs
{
    public class ClothePhotoCreateDTO
    {
        public IFormFile? Photo { get; set; }
        public Guid ColorId { get; set; }   
        public bool IsMain { get; set; }
    }
}
