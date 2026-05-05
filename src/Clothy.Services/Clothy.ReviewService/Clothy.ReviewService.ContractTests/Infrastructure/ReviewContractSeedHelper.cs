using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace Clothy.ReviewService.ContractTests.Infrastructure;

public static class ReviewContractSeedHelper
{
    public static async Task<SeedData> SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        IReviewRepository reviewRepo = scope.ServiceProvider.GetRequiredService<IReviewRepository>();
        IQuestionRepository questionRepo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();

        Guid clotheId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        ClotheInfo clotheInfo = new ClotheInfo(clotheId, "Contract Clothe", "https://test.com/photo.jpg");
        UserInfo userInfo = new UserInfo(userId, "John", "Doe", "https://test.com/avatar.jpg");

        Review review = new Review(clotheInfo, userInfo, 5, "Great product!");
        review.ConfirmStatus();
        await reviewRepo.AddAsync(review);

        Question question = new Question(clotheInfo, userInfo, "Is it good quality?");
        question.AddAnswer(new Answer(userInfo, "Yes, very good!"));
        await questionRepo.AddAsync(question);

        return new SeedData(clotheId, userId);
    }

    public record SeedData(Guid ClotheId, Guid UserId);
}