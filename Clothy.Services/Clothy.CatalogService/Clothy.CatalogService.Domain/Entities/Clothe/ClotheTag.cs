using Clothy.CatalogService.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Clothe
{
    public class ClotheTag
    {
        public Guid ClotheId { get; set; }
        public ClotheItem? Clothe { get; set; } 
        public Guid TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}