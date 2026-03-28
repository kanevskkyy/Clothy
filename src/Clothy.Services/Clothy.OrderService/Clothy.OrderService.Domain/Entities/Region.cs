using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.Domain.Entities
{
    public class Region : BaseEntity
    {
        public string? Name { get; set; }
        public string? Ref {  get; set; }

        public ICollection<Settlement> Settlements = new List<Settlement>(); 
    }
}
