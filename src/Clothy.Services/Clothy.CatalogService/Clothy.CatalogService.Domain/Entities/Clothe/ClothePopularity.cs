using Clothy.CatalogService.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.Domain.Entities.Clothe
{
    public class ClothePopularity : BaseEntity
    {
        public Guid ClotheId { get; set; }
        public ClotheItem? ClotheItem { get; set; }

        public int SoldCount { get; set; }
    }
}