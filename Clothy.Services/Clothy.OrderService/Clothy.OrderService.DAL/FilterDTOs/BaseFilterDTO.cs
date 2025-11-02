using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.DAL.FilterDTOs
{
    public class BaseFilterDTO
    {
        private const int MAX_PAGE_SIZE = 50;
        private int PAGE_SIZE = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => PAGE_SIZE;
            set => PAGE_SIZE = (value > MAX_PAGE_SIZE) ? MAX_PAGE_SIZE : value;
        }

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
