using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.ReviewService.Domain.ValueObjects
{
    public class ClotheInfo : ValueObject
    {
        [BsonElement("clotheItemId")]
        public Guid ClotheItemId { get; }

        [BsonElement("clotheName")]
        public string? ClotheName { get; }

        [BsonElement("clothePhotoURL")]
        public string? ClothePhotoURL { get; }

        [JsonConstructor]
        public ClotheInfo(Guid clotheItemId, string clotheName, string clothePhotoURL)
        {
            ClotheItemId = clotheItemId;
            ClotheName = clotheName;  
            ClothePhotoURL = clothePhotoURL;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ClotheItemId;
            yield return ClotheName!;
            yield return ClothePhotoURL!;
        }
    }
}
