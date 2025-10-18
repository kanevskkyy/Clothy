using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Exceptions;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.Entities
{
    public class Review : BaseEntity
    {
        [BsonElement("clotheItemId")]
        public Guid ClotheItemId { get; private set; }

        [BsonElement("userId")]
        public Guid UserId { get; private set; }

        [BsonElement("userFirstName")]
        public string UserFirstName { get; private set; }

        [BsonElement("userLastName")]
        public string UserLastName { get; private set; }

        [BsonElement("userPhotoUrl")]
        public string UserPhotoUrl { get; private set; }

        [BsonElement("rating")]
        public int Rating { get; private set; }

        [BsonElement("comment")]
        public string Comment { get; private set; }

        private Review() 
        {

        } 
        public Review(Guid clotheItemId, Guid userId, string firstName, string lastName, int rating, string comment, string photoUrl)
        {
            if (rating < 1 || rating > 5) throw new InvalidRatingException(rating);

            if (string.IsNullOrWhiteSpace(comment)) throw new EmptyCommentException();

            ClotheItemId = clotheItemId;
            UserId = userId;
            UserFirstName = firstName.Trim();
            UserLastName = lastName.Trim();
            Rating = rating;
            Comment = comment.Trim();
            UserPhotoUrl = photoUrl;
        }

        public void UpdateComment(string newComment, int? newRating = null)
        {
            if (string.IsNullOrWhiteSpace(newComment)) throw new EmptyCommentException();

            Comment = newComment.Trim();

            if (newRating.HasValue)
            {
                if (newRating < 1 || newRating > 5) throw new InvalidRatingException(newRating);
                Rating = newRating.Value;
            }

            UpdateTimestamp();
        }
    }
}
