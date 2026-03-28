using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;
using MongoDB.Driver;

namespace Clothy.ReviewService.Infrastructure.DB.Seeding
{
    public class ReviewSeeder : IDataSeeder
    {
        private IMongoCollection<Review> reviews;

        public ReviewSeeder(MongoDbContext context)
        {
            reviews = context.Reviews;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var existingCount = await reviews.CountDocumentsAsync(FilterDefinition<Review>.Empty, cancellationToken: cancellationToken);
            if (existingCount > 0) return;

            List<Review> fakeData = new List<Review>
            {
                new Review(
                    new ClotheInfo(Guid.NewGuid(), "Test clothe 1", "https://randomPhotoURL"),
                    new UserInfo(Guid.NewGuid(), "Alice", "Smith", "https://randomuser.me/api/portraits/women/1.jpg"),
                    5,
                    "Absolutely love it! Great quality and fit."
                ),
                new Review(
                    new ClotheInfo(Guid.NewGuid(), "Test clothe 2", "https://randomPhotoURL"),
                    new UserInfo(Guid.NewGuid(), "Bob", "Johnson", "https://randomuser.me/api/portraits/men/2.jpg"),
                    4,
                    "Pretty good, but shipping was slow."
                ),
                new Review(
                    new ClotheInfo(Guid.NewGuid(), "Test clothe 3", "https://randomPhotoURL"),
                    new UserInfo(Guid.NewGuid(), "Clara", "Davis", "https://randomuser.me/api/portraits/women/3.jpg"),
                    3,
                    "Average product, nothing special."
                ),
                new Review(
                    new ClotheInfo(Guid.NewGuid(), "Test clothe 4", "https://randomPhotoURL"),
                    new UserInfo(Guid.NewGuid(), "David", "Brown", "https://randomuser.me/api/portraits/men/4.jpg"),
                    5,
                    "Excellent! Would buy again."
                ),
                new Review(
                    new ClotheInfo(Guid.NewGuid(), "Test clothe 5", "https://randomPhotoURL"),
                    new UserInfo(Guid.NewGuid(), "Eva", "Miller", "https://randomuser.me/api/portraits/women/5.jpg"),
                    2,
                    "Not satisfied, the material feels cheap."
                ),
                new Review(
                    new ClotheInfo(Guid.NewGuid(), "Test clothe 6", "https://randomPhotoURL"),
                    new UserInfo(Guid.NewGuid(), "Frank", "Wilson", "https://randomuser.me/api/portraits/men/6.jpg"),
                    4,
                    "Good product, reasonable price."
                )
            };

            for(int i = 0; i < fakeData.Count; i++)
            {
                if (i % 2 == 0) fakeData[i].ConfirmStatus();
            }

            await reviews.InsertManyAsync(fakeData, cancellationToken: cancellationToken);
        }
    }
}
