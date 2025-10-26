using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;
using MongoDB.Bson.Serialization;

namespace Clothy.ReviewService.Infrastructure.DB.MappingConfig
{
    public static class ValueObjectMappings
    {
        public static void Register()
        {
            BsonClassMap.RegisterClassMap<UserInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapCreator(u => new UserInfo(u.UserId, u.FirstName, u.LastName, u.PhotoUrl));
            });

            BsonClassMap.RegisterClassMap<Answer>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Question>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Review>(cm => cm.AutoMap());
        }
    }
}
