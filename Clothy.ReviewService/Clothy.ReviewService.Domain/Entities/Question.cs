using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Exceptions;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.Entities
{
    public class Question : BaseEntity
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

        [BsonElement("questionText")]
        public string QuestionText { get; private set; }

        [BsonElement("answers")]
        public List<Answer> Answers { get; private set; } = new();

        private Question() 
        {
            
        }

        public Question(Guid clotheItemId, Guid userId, string firstName, string lastName, string questionText, string photoUrl)
        {
            if (string.IsNullOrWhiteSpace(questionText)) throw new EmptyQuestionException();

            ClotheItemId = clotheItemId;
            UserId = userId;
            UserFirstName = firstName.Trim();
            UserLastName = lastName.Trim();
            UserPhotoUrl = photoUrl;
            QuestionText = questionText.Trim();
        }

        public void AddAnswer(Guid userId, string firstName, string lastName, string answerText, string photoUrl)
        {
            if (string.IsNullOrWhiteSpace(answerText)) throw new EmptyAnswerException();

            Answer answer = new Answer(userId, firstName, lastName, answerText, photoUrl);
            Answers.Add(answer);
            UpdateTimestamp();
        }
    }
}
