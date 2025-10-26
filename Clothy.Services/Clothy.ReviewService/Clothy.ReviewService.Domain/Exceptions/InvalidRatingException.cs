using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.Exceptions
{
    public class InvalidRatingException : DomainException
    {
        public InvalidRatingException(int? providedRating) : base($"Rating must be between 1 and 5. Provided: {(providedRating.HasValue ? providedRating.Value.ToString() : "null")}")
        {

        }
    }
}
