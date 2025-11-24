using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewDTO
    {
        public Guid ClotheItemId { get; set; }
        public int Rating { get; set; }
        public string Comment {  get; set; }
    }
}
