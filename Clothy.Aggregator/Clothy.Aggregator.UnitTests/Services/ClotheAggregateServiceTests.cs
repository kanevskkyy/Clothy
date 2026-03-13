using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs;
using Clothy.Aggregator.Aggregate.Services;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Clothy.Aggregator.UnitTests.Services;

public class ClotheAggregateServiceTests
{
    private readonly Mock<IClotheGrpcClient> clotheGrpcClientMock;
    private readonly Mock<IReviewGrpcClient> reviewGrpcClientMock;
    private readonly Mock<ILogger<ClotheAggregateService>> loggerMock;

    private readonly ClotheAggregateService service;

    public ClotheAggregateServiceTests()
    {
        clotheGrpcClientMock = new Mock<IClotheGrpcClient>();
        reviewGrpcClientMock = new Mock<IReviewGrpcClient>();
        loggerMock = new Mock<ILogger<ClotheAggregateService>>();

        service = new ClotheAggregateService(
            clotheGrpcClientMock.Object,
            reviewGrpcClientMock.Object,
            loggerMock.Object);
    }

    private ClotheDetailGrpcResponse BuildClotheResponse(Guid? id = null)
    {
        return new ClotheDetailGrpcResponse
        {
            Id = (id ?? Guid.NewGuid()).ToString(),
            Name = "Test Clothe",
            Slug = "test-clothe"
        };
    }

    private ReviewsListGrpcResponse BuildReviewsResponse(int count = 2)
    {
        ReviewsListGrpcResponse response = new ReviewsListGrpcResponse
        {
            CurrentPage = 1,
            TotalPages = 1,
            PageSize = 10,
            TotalCount = count
        };
        for (int i = 0; i < count; i++)
            response.Items.Add(new ReviewGrpcResponse { Id = Guid.NewGuid().ToString() });
        return response;
    }

    private QuestionsListGrpcResponse BuildQuestionsResponse(int count = 1)
    {
        QuestionsListGrpcResponse response = new QuestionsListGrpcResponse
        {
            CurrentPage = 1,
            TotalPages = 1,
            PageSize = 10,
            TotalCount = count
        };
        for (int i = 0; i < count; i++)
            response.Items.Add(new QuestionGrpcResponse { Id = Guid.NewGuid().ToString() });
        return response;
    }

    private ReviewStatisticGrpcResponse BuildStatsResponse()
    {
        return new ReviewStatisticGrpcResponse
        {
            AverageRating = 4.5,
            TotalReviews = 10
        };
    }

    private void SetupAllClients(ClotheDetailGrpcResponse clothe)
    {
        Guid clotheId = Guid.Parse(clothe.Id);

        clotheGrpcClientMock
            .Setup(c => c.GetClotheByIdAsync(clothe.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clothe);

        reviewGrpcClientMock
            .Setup(c => c.GetReviewsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildReviewsResponse());

        reviewGrpcClientMock
            .Setup(c => c.GetQuestionsAndAnswersByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildQuestionsResponse());

        reviewGrpcClientMock
            .Setup(c => c.GetStatisticsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildStatsResponse());
    }

    [Fact]
    public async Task GetFullClotheDetailAsync_WhenAllClientsSucceed_ReturnsClotheDetailFullDTO()
    {
        ClotheDetailGrpcResponse clothe = BuildClotheResponse();
        SetupAllClients(clothe);

        ClotheDetailFullDTO result = await service.GetFullClotheDetailAsync(clothe.Slug);

        result.Should().NotBeNull();
        result.ClotheDetailDTO.Should().Be(clothe);
        result.Reviews.Should().NotBeNull();
        result.Questions.Should().NotBeNull();
        result.Statistics.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFullClotheDetailAsync_WhenAllClientsSucceed_MapsReviewsCorrectly()
    {
        ClotheDetailGrpcResponse clothe = BuildClotheResponse();
        ReviewsListGrpcResponse reviewsResponse = BuildReviewsResponse(count: 3);
        Guid clotheId = Guid.Parse(clothe.Id);

        clotheGrpcClientMock
            .Setup(c => c.GetClotheByIdAsync(clothe.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clothe);

        reviewGrpcClientMock
            .Setup(c => c.GetReviewsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewsResponse);

        reviewGrpcClientMock
            .Setup(c => c.GetQuestionsAndAnswersByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildQuestionsResponse());

        reviewGrpcClientMock
            .Setup(c => c.GetStatisticsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildStatsResponse());

        ClotheDetailFullDTO result = await service.GetFullClotheDetailAsync(clothe.Slug);

        result.Reviews.Items.Should().HaveCount(3);
        result.Reviews.TotalCount.Should().Be(3);
        result.Reviews.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task GetFullClotheDetailAsync_WhenAllClientsSucceed_MapsStatisticsCorrectly()
    {
        ClotheDetailGrpcResponse clothe = BuildClotheResponse();
        SetupAllClients(clothe);

        ClotheDetailFullDTO result = await service.GetFullClotheDetailAsync(clothe.Slug);

        result.Statistics.AverageRating.Should().Be(4.5);
        result.Statistics.TotalReviews.Should().Be(10);
    }

    [Fact]
    public async Task GetFullClotheDetailAsync_CallsAllThreeReviewClientMethods()
    {
        ClotheDetailGrpcResponse clothe = BuildClotheResponse();
        Guid clotheId = Guid.Parse(clothe.Id);
        SetupAllClients(clothe);

        await service.GetFullClotheDetailAsync(clothe.Slug);

        reviewGrpcClientMock.Verify(c => c.GetReviewsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()), Times.Once);
        reviewGrpcClientMock.Verify(c => c.GetQuestionsAndAnswersByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()), Times.Once);
        reviewGrpcClientMock.Verify(c => c.GetStatisticsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFullClotheDetailAsync_WhenClotheClientThrows_PropagatesException()
    {
        clotheGrpcClientMock
            .Setup(c => c.GetClotheByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Clothe not found"));

        Func<Task> act = async () => await service.GetFullClotheDetailAsync("non-existent-slug");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetFullClotheDetailAsync_WhenReviewClientThrows_PropagatesException()
    {
        ClotheDetailGrpcResponse clothe = BuildClotheResponse();
        Guid clotheId = Guid.Parse(clothe.Id);

        clotheGrpcClientMock
            .Setup(c => c.GetClotheByIdAsync(clothe.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clothe);

        reviewGrpcClientMock
            .Setup(c => c.GetReviewsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Review service unavailable"));

        reviewGrpcClientMock
            .Setup(c => c.GetQuestionsAndAnswersByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildQuestionsResponse());

        reviewGrpcClientMock
            .Setup(c => c.GetStatisticsByClotheIdAsync(clotheId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildStatsResponse());

        Func<Task> act = async () => await service.GetFullClotheDetailAsync(clothe.Slug);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Review service unavailable*");
    }
}