using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.APIClientDTOs
{
    public class NovaPoshtaResponse<T>
    {
        public bool Success { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
