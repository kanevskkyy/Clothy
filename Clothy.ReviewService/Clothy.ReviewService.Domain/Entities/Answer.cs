using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Exceptions;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.Entities
{
    public class Answer
    {
        [BsonElement("answerId")]
        public Guid AnswerId { get; private set; } = Guid.NewGuid();

        [BsonElement("userId")]
        public Guid UserId { get; private set; }

        [BsonElement("userFirstName")]
        public string UserFirstName { get; private set; }

        [BsonElement("userLastName")]
        public string UserLastName { get; private set; }

        [BsonElement("userPhotoUrl")]
        public string UserPhotoUrl { get; private set; }

        [BsonElement("answerText")]
        public string AnswerText { get; private set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        private Answer() 
        {
            
        }

        public Answer(Guid userId, string firstName, string lastName, string text, string photoUrl)
        {
            UserId = userId;
            UserFirstName = firstName.Trim();
            UserLastName = lastName.Trim();
            UserPhotoUrl = photoUrl;
            AnswerText = text.Trim();
        }

        public void UpdateAnswer(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText)) throw new EmptyAnswerException();

            AnswerText = newText.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
