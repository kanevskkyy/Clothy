using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.MaterialDTOs
{
    public class MaterialWithCountDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public int Count { get; set; }
    }
}
