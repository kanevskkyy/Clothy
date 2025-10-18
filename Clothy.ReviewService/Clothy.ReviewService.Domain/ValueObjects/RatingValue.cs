using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Exceptions;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.ValueObjects
{
    public class RatingValue : ValueObject
    {
        [BsonElement("rating")]
        public int Rating { get; }

        public RatingValue(int rating)
        {
            if (rating < 1 || rating > 5) throw new InvalidRatingException(rating);
            Rating = rating;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Rating;
        }
    }
}
