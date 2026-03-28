using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.ValueObjects
{
    public class ReviewStatistics
    {
        public Guid ClotheItemId { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
        public double AverageRating { get; set; }
    }
}
