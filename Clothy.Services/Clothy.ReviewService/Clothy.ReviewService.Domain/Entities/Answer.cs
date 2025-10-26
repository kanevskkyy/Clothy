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
    public class Answer : BaseEntity
    {
        [BsonElement("user")]
        public UserInfo User { get; private set; }

        [BsonElement("answerText")]
        public string AnswerText { get; private set; }  

        private Answer() 
        {

        }

        public Answer(UserInfo user, string answerText)
        {
            if (string.IsNullOrWhiteSpace(answerText)) throw new EmptyValueException("AnswerText");

            User = user;
            AnswerText = answerText.Trim();
        }

        public void UpdateAnswer(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText)) throw new EmptyValueException("AnswerText");

            AnswerText = newText.Trim();
            UpdateTimestamp();
        }
    }
}
