using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Exceptions;
using Clothy.ReviewService.Domain.ValueObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.Entities
{
    public class Review : BaseEntity
    {
        [BsonElement("clotheItemId")]
        public Guid ClotheItemId { get; private set; }

        [BsonElement("user")]
        public UserInfo User { get; private set; }

        [BsonElement("rating")]
        public RatingValue Rating { get; private set; }

        [BsonElement("comment")]
        public TextValue Comment { get; private set; }

        private Review() 
        {
            
        }

        public Review(Guid clotheItemId, UserInfo user, RatingValue rating, TextValue comment)
        {
            ClotheItemId = clotheItemId;
            User = user;
            Rating = rating;
            Comment = comment;
        }

        public void UpdateComment(TextValue newComment, RatingValue? newRating = null)
        {
            Comment = newComment;
            if (newRating != null) Rating = newRating;
            UpdateTimestamp();
        }
    }
}
