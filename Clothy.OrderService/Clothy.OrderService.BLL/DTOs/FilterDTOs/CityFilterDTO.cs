using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.BLL.DTOs.FilterDTOs
{
    public class CityFilterDTO
    {
        public string? Name { get; set; }
        public string SortBy { get; set; } = "Name"; 
        public bool SortDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
