using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class ClotheMaterial
    {
        public Guid ClotheId { get; set; }
        public ClotheItem? Clothe { get; set; }

        public Guid MaterialId { get; set; }
        public Material? Material { get; set; }

        public int Percentage { get; set; }
    }
}
