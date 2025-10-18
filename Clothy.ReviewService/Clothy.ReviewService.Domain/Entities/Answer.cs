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
        public TextValue AnswerText { get; private set; }

        private Answer() 
        {
            
        }

        public Answer(UserInfo user, TextValue text)
        {
            User = user;
            AnswerText = text;
        }

        public void UpdateAnswer(TextValue newText)
        {
            AnswerText = newText; 
            UpdateTimestamp();
        }
    }
}
