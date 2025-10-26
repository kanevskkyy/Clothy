using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.Entities.QueryParameters
{
    public class QuestionQueryParameters : QueryStringParameters
    {
        public Guid? UserId { get; set; }
        public Guid? ClotheItemId { get; set; }
    }
}
