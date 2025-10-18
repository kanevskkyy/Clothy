using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Exceptions;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.ValueObjects
{
    public class TextValue : ValueObject
    {
        [BsonElement("text")]
        public string Text { get; }

        public TextValue(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new EmptyValueException(fieldName);

            Text = text.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Text;
        }
    }
}
