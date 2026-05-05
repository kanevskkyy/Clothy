using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Clothy.Aggregator.Aggregate.DTOs;
using Clothy.Aggregator.Aggregate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Clothy.Aggregator.UnitTests.Services;

public class DashboardAggregateServiceTests
{
    private readonly Mock<IOrderGrpcClient> orderGrpcClientMock;
    private readonly Mock<IStockGrpcClient> stockGrpcClientMock;
    private readonly Mock<IReviewGrpcClient> reviewGrpcClientMock;
    private readonly Mock<ILogger<DashboardAggregateService>> loggerMock;

    private readonly DashboardAggregateService service;

    public DashboardAggregateServiceTests()
    {
        orderGrpcClientMock = new Mock<IOrderGrpcClient>();
        stockGrpcClientMock = new Mock<IStockGrpcClient>();
        reviewGrpcClientMock = new Mock<IReviewGrpcClient>();
        loggerMock = new Mock<ILogger<DashboardAggregateService>>();

        service = new DashboardAggregateService(
            orderGrpcClientMock.Object,
            stockGrpcClientMock.Object,
            reviewGrpcClientMock.Object,
            loggerMock.Object);
    }

    private void SetupAllClients(
        int newOrders = 5,
        string totalPrice = "1500.00",
        int pendingOrders = 3,
        int totalQuantity = 200,
        int pendingReviews = 4,
        int questionsWithoutAnswer = 2)
    {
        orderGrpcClientMock
            .Setup(c => c.GetOrderStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderStats
            {
                NewOrdersCount = newOrders,
                TotalPrice = totalPrice,
                PendingOrders = pendingOrders
            });

        stockGrpcClientMock
            .Setup(c => c.GetTotalQuantityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetTotalQuantityResponse
            {
                TotalQuantity = totalQuantity
            });

        reviewGrpcClientMock
            .Setup(c => c.GetPendingReviewsCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PendingReviewsCountGrpcResponse
            {
                ReviewsCount = pendingReviews
            });

        reviewGrpcClientMock
            .Setup(c => c.GetQuestionsWithoutAnswerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionsWithoutAnswerGrpcResponse
            {
                QuestionsCount = questionsWithoutAnswer
            });
    }

    [Fact]
    public async Task GetDashboardStatisticsAsync_WhenAllClientsSucceed_ReturnsDashboardFullDTO()
    {
        SetupAllClients();

        DashboardFullDTO result = await service.GetDashboardStatisticsAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardStatisticsAsync_WhenAllClientsSucceed_MapsDTOCorrectly()
    {
        SetupAllClients(
            newOrders: 5,
            totalPrice: "1500.00",
            pendingOrders: 3,
            totalQuantity: 200,
            pendingReviews: 4,
            questionsWithoutAnswer: 2);

        DashboardFullDTO result = await service.GetDashboardStatisticsAsync();

        result.NewOrdersCount.Should().Be(5);
        result.TotalPrice.Should().Be(1500.00m);
        result.PendingOrdersCount.Should().Be(3);
        result.TotalItemsCount.Should().Be(200);
        result.PendingReviewCount.Should().Be(4);
        result.QuestionsWithoutAnswerCount.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardStatisticsAsync_CallsAllFourClientMethods()
    {
        SetupAllClients();

        await service.GetDashboardStatisticsAsync();

        orderGrpcClientMock.Verify(c => c.GetOrderStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        stockGrpcClientMock.Verify(c => c.GetTotalQuantityAsync(It.IsAny<CancellationToken>()), Times.Once);
        reviewGrpcClientMock.Verify(c => c.GetPendingReviewsCountAsync(It.IsAny<CancellationToken>()), Times.Once);
        reviewGrpcClientMock.Verify(c => c.GetQuestionsWithoutAnswerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardStatisticsAsync_WhenOrderClientThrows_PropagatesException()
    {
        orderGrpcClientMock
            .Setup(c => c.GetOrderStatsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Order service unavailable"));

        stockGrpcClientMock
            .Setup(c => c.GetTotalQuantityAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetTotalQuantityResponse { TotalQuantity = 100 });

        reviewGrpcClientMock
            .Setup(c => c.GetPendingReviewsCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PendingReviewsCountGrpcResponse { ReviewsCount = 1 });

        reviewGrpcClientMock
            .Setup(c => c.GetQuestionsWithoutAnswerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionsWithoutAnswerGrpcResponse { QuestionsCount = 1 });

        Func<Task> act = async () => await service.GetDashboardStatisticsAsync();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Order service unavailable*");
    }

    [Fact]
    public async Task GetDashboardStatisticsAsync_WhenStockClientThrows_PropagatesException()
    {
        orderGrpcClientMock
            .Setup(c => c.GetOrderStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderStats { NewOrdersCount = 1, TotalPrice = "100.00", PendingOrders = 1 });

        stockGrpcClientMock
            .Setup(c => c.GetTotalQuantityAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Stock service unavailable"));

        reviewGrpcClientMock
            .Setup(c => c.GetPendingReviewsCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PendingReviewsCountGrpcResponse { ReviewsCount = 1 });

        reviewGrpcClientMock
            .Setup(c => c.GetQuestionsWithoutAnswerAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionsWithoutAnswerGrpcResponse { QuestionsCount = 1 });

        Func<Task> act = async () => await service.GetDashboardStatisticsAsync();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Stock service unavailable*");
    }

    [Fact]
    public async Task GetDashboardStatisticsAsync_WhenTotalPriceIsZero_MapsCorrectly()
    {
        SetupAllClients(totalPrice: "0.00");

        DashboardFullDTO result = await service.GetDashboardStatisticsAsync();

        result.TotalPrice.Should().Be(0m);
    }
}