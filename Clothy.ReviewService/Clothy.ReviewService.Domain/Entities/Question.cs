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
    public class Question : BaseEntity
    {
        [BsonElement("clotheItemId")]
        public Guid ClotheItemId { get; private set; }

        [BsonElement("user")]
        public UserInfo User { get; private set; }

        [BsonElement("questionText")]
        public TextValue QuestionText { get; private set; }

        [BsonElement("answers")]
        public List<Answer> Answers { get; private set; } = new();

        private Question() { }

        public Question(Guid clotheItemId, UserInfo user, TextValue text)
        {
            ClotheItemId = clotheItemId;
            User = user;
            QuestionText = text;
        }

        public void AddAnswer(Answer answer)
        {
            if (answer == null) throw new EmptyValueException("Answer");
            Answers.Add(answer);
            UpdateTimestamp();
        }
    }
}
