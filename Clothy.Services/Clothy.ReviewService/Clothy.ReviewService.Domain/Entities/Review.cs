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
        public int Rating { get; private set; }  

        [BsonElement("comment")]
        public string Comment { get; private set; }  

        private Review() 
        {
            
        }

        public Review(Guid clotheItemId, UserInfo user, int rating, string comment)
        {
            if (rating < 1 || rating > 5) throw new InvalidRatingException(rating);

            if (string.IsNullOrWhiteSpace(comment)) throw new EmptyValueException("Comment");

            ClotheItemId = clotheItemId;
            User = user;
            Rating = rating;
            Comment = comment.Trim();
        }

        public void UpdateComment(string newComment, int newRating)
        {
            if (string.IsNullOrWhiteSpace(newComment)) throw new EmptyValueException("Comment");

            if (newRating < 1 || newRating > 5) throw new InvalidRatingException(newRating);

            Comment = newComment.Trim();
            Rating = newRating;
            UpdateTimestamp();
        }
    }
}