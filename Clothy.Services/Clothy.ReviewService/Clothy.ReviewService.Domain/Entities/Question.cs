using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string QuestionText { get; private set; }  

        [BsonElement("answers")]
        public List<Answer> Answers { get; private set; } = new();

        private Question() 
        {

        }

        public Question(Guid clotheItemId, UserInfo user, string questionText)
        {
            ClotheItemId = clotheItemId;
            User = user;
            QuestionText = questionText.Trim();
        }

        public void UpdateQuestion(string newText)
        {
            QuestionText = newText.Trim();
            UpdateTimestamp();
        }

        public void AddAnswer(Answer answer)
        {
            if (answer == null) throw new ArgumentNullException(nameof(answer));

            Answers.Add(answer);
            UpdateTimestamp();
        }
    }
}