using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.DTOs
{
    public class DashboardFullDTO
    {
        public int NewOrdersCount { get; set; }
        public decimal TotalPrice { get; set; }
        public int PendingOrdersCount { get; set; }
        public int TotalItemsCount { get; set; }
        public int PendingReviewCount { get; set; }
        public int QuestionsWithoutAnswerCount { get; set; }
    }
}
