using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;
using MongoDB.Bson.Serialization;

namespace Clothy.ReviewService.Infrastructure.DB.MappingConfig
{
    public static class ValueObjectMappings
    {
        private static Lazy<bool> _registered = new(DoRegister, LazyThreadSafetyMode.ExecutionAndPublication);

        public static void Register() => _ = _registered.Value;

        private static bool DoRegister()
        {
            BsonClassMap.RegisterClassMap<UserInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapCreator(userInfo => new UserInfo(userInfo.UserId, userInfo.FirstName, userInfo.LastName, userInfo.PhotoUrl));
            });

            BsonClassMap.RegisterClassMap<ClotheInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapCreator(clotheInfo => new ClotheInfo(clotheInfo.ClotheItemId, clotheInfo.ClotheName, clotheInfo.ClothePhotoURL));
            });

            BsonClassMap.RegisterClassMap<Answer>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Question>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<Review>(cm => cm.AutoMap());

            return true;
        }
    }
}