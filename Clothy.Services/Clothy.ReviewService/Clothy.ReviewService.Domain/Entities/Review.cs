using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [BsonElement("status")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public ReviewStatus Status { get; private set; }

        private Review() 
        {
            
        }

        public Review(Guid clotheItemId, UserInfo user, int rating, string comment)
        {
            ClotheItemId = clotheItemId;
            User = user;
            Rating = rating;
            Comment = comment.Trim();
            Status = ReviewStatus.Pending;
        }

        public void ConfirmStatus()
        {
            Status = ReviewStatus.Confirmed;
            UpdateTimestamp();
        }


        public void UpdateComment(string newComment, int newRating)
        {
            Comment = newComment.Trim();
            Rating = newRating;
            UpdateTimestamp();
        }
    }
}