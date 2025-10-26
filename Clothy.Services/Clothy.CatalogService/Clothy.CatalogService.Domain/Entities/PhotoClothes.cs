using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities
{
    public class PhotoClothes : BaseEntity
    {
        public Guid ClotheId { get; set; }
        public string? PhotoURL { get; set; }

        public ClotheItem? Clothe { get; set; }
    }
}
