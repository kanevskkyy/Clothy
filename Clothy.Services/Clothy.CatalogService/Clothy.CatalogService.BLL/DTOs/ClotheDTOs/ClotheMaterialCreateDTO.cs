using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheMaterialCreateDTO
    {
        public Guid MaterialId { get; set; }
        public int Percentage { get; set; }
    }
}
