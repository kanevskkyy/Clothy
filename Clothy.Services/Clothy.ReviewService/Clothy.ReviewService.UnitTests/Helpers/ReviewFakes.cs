using Bogus;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;

namespace Clothy.ReviewService.UnitTests.Helpers;

public static class ReviewFakes
{
    private static Faker faker = new();
 
    public static Review CreateReview(Guid? userId = null)
    {
        UserInfo userInfo = new UserInfo(
            userId ?? Guid.NewGuid(),
            faker.Name.FirstName(),
            faker.Name.LastName(),
            faker.Internet.Avatar()
        );
 
        ClotheInfo clotheInfo = new ClotheInfo(
            Guid.NewGuid(),
            faker.Commerce.ProductName(),
            faker.Internet.Avatar()
        );
 
        return new Review(clotheInfo, userInfo, faker.Random.Int(1, 5), faker.Lorem.Sentence());
    }
 
    public static Review CreatePendingReview(Guid? userId = null)
    {
        Review review = CreateReview(userId);
        return review;
    }
 
    public static Review CreateConfirmedReview(Guid? userId = null)
    {
        Review review = CreateReview(userId);
        review.ConfirmStatus();
        
        return review;
    }
}