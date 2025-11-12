using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Clothy.ReviewService.Domain.ValueObjects
{
    public class UserInfo : ValueObject
    {
        [BsonElement("userId")]
        public Guid UserId { get; }

        [BsonElement("firstName")]
        public string FirstName { get; }

        [BsonElement("lastName")]
        public string LastName { get; }

        [BsonElement("photoUrl")]
        public string PhotoUrl { get; }

        [JsonConstructor]
        public UserInfo(Guid userId, string firstName, string lastName, string photoUrl)
        {
            UserId = userId;
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            PhotoUrl = photoUrl;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return UserId;
            yield return FirstName;
            yield return LastName;
            yield return PhotoUrl;
        }
    }
}
