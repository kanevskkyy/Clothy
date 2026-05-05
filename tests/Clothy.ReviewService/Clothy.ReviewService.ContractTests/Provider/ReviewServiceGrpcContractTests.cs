using Clothy.ReviewService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Xunit;

namespace Clothy.ReviewService.ContractTests.Provider;

[Collection("ReviewContract")]
public class ReviewServiceGrpcContractTests : IAsyncLifetime
{
    private ReviewGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private ReviewContractSeedHelper.SeedData seed = null!;

    public ReviewServiceGrpcContractTests(ReviewGrpcWebFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        channel = factory.CreateGrpcChannel();
        seed = await ReviewContractSeedHelper.SeedAsync(factory.Services);
    }

    public Task DisposeAsync()
    {
        channel.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetReviewsByClotheId_WhenReviewsExist_ReturnsReviewsList()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        ReviewsListGrpcResponse response = await client.GetReviewsByClotheIdAsync(new ReviewClotheIdGrpcRequest
        {
            Id = seed.ClotheId.ToString()
        });

        Assert.True(response.TotalCount > 0);
        Assert.NotEmpty(response.Items);
        Assert.Equal(5, response.Items[0].Rating);
        Assert.Equal("Great product!", response.Items[0].Comment);
    }

    [Fact]
    public async Task GetReviewsByClotheId_ResponseContract_HasRequiredFields()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        ReviewsListGrpcResponse response = await client.GetReviewsByClotheIdAsync(new ReviewClotheIdGrpcRequest
        {
            Id = seed.ClotheId.ToString()
        });

        Assert.True(response.CurrentPage > 0);
        Assert.True(response.PageSize > 0);
        Assert.True(response.TotalCount >= 0);

        if (response.Items.Any())
        {
            ReviewGrpcResponse item = response.Items[0];
            Assert.False(string.IsNullOrEmpty(item.Id));
            Assert.NotNull(item.User);
            Assert.False(string.IsNullOrEmpty(item.User.Id));
            Assert.True(item.Rating is >= 1 and <= 5);
            Assert.False(string.IsNullOrEmpty(item.Comment));
            Assert.False(string.IsNullOrEmpty(item.CreatedAt));
        }
    }

    [Fact]
    public async Task GetStatisticsByClotheId_WhenReviewsExist_ReturnsStats()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        ReviewStatisticGrpcResponse response = await client.GetStatisticsByClotheIdAsync(new ReviewClotheIdGrpcRequest
        {
            Id = seed.ClotheId.ToString()
        });

        Assert.Equal(seed.ClotheId.ToString(), response.ClotheItemId);
        Assert.True(response.TotalReviews > 0);
        Assert.True(response.AverageRating > 0);
        Assert.Equal(1, response.FiveStars);
    }

    [Fact]
    public async Task GetStatisticsByClotheId_ResponseContract_HasRequiredFields()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        ReviewStatisticGrpcResponse response = await client.GetStatisticsByClotheIdAsync(new ReviewClotheIdGrpcRequest
            {
                Id = seed.ClotheId.ToString()
            });

        Assert.False(string.IsNullOrEmpty(response.ClotheItemId));
        Assert.IsType<int>(response.TotalReviews);
        Assert.IsType<int>(response.FiveStars);
        Assert.IsType<int>(response.FourStars);
        Assert.IsType<int>(response.ThreeStars);
        Assert.IsType<int>(response.TwoStars);
        Assert.IsType<int>(response.OneStars);
        Assert.IsType<double>(response.AverageRating);
    }

    [Fact]
    public async Task GetAnswersAndQuestionByClotheId_WhenQuestionsExist_ReturnsQuestions()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        QuestionsListGrpcResponse response = await client.GetAnswersAndQuestionByClotheIdAsync(new ReviewClotheIdGrpcRequest
            {
                Id = seed.ClotheId.ToString()
            });

        Assert.True(response.TotalCount > 0);
        Assert.NotEmpty(response.Items);
        Assert.Equal("Is it good quality?", response.Items[0].QuestionText);
        Assert.NotEmpty(response.Items[0].Answers);
        Assert.Equal("Yes, very good!", response.Items[0].Answers[0].AnswerText);
    }

    [Fact]
    public async Task GetAnswersAndQuestionByClotheId_ResponseContract_HasRequiredFields()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        QuestionsListGrpcResponse response = await client.GetAnswersAndQuestionByClotheIdAsync(
            new ReviewClotheIdGrpcRequest
            {
                Id = seed.ClotheId.ToString()
            });

        Assert.True(response.CurrentPage > 0);
        Assert.True(response.PageSize > 0);

        if (response.Items.Any())
        {
            QuestionGrpcResponse q = response.Items[0];
            Assert.False(string.IsNullOrEmpty(q.Id));
            Assert.NotNull(q.User);
            Assert.False(string.IsNullOrEmpty(q.QuestionText));
            Assert.False(string.IsNullOrEmpty(q.CreatedAt));
        }
    }

    [Fact]
    public async Task GetPendingReviewsCount_WhenCalled_ReturnsNonNegative()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        PendingReviewsCountGrpcResponse response = await client.GetPendingReviewsCountAsync(new Google.Protobuf.WellKnownTypes.Empty());

        Assert.True(response.ReviewsCount >= 0);
        Assert.IsType<int>(response.ReviewsCount);
    }

    [Fact]
    public async Task GetQuestionsCountWithoutAnswer_WhenCalled_ReturnsNonNegative()
    {
        ReviewServiceGrpc.ReviewServiceGrpcClient client = new(channel);

        QuestionsWithoutAnswerGrpcResponse response = await client.GetQuestionsCountWithoutAnswerAsync(new Google.Protobuf.WellKnownTypes.Empty());

        Assert.True(response.QuestionsCount >= 0);
        Assert.IsType<int>(response.QuestionsCount);
    }
}