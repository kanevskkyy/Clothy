using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.PhotoDTOs
{
    public class ClotheColorSummaryDTO
    {
        public Guid Id { get; set; }
        public string? MainPhotoURL { get; set; }
        public Guid ColorId { get; set; }
        public string? HexCode { get; set; }
    }
}
