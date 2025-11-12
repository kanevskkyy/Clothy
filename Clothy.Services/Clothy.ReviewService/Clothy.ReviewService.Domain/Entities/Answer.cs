using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            User = user;
            AnswerText = answerText.Trim();
        }

        public void UpdateAnswer(string newText)
        {
            AnswerText = newText.Trim();
            UpdateTimestamp();
        }
    }
}
