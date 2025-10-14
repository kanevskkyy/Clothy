using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.ColorDTOs
{
    public class ColorReadDTO
    {
        public Guid Id { get; set; }
        public string HexCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
