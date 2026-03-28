using Clothy.CatalogService.BLL.Services;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.Shared.Events.EmailEvents.ClotheStockUpdated;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Clothy.CatalogService.UnitTests.Services;

public class StockNotificationServiceTests
{
    private Mock<IUnitOfWork> unitOfWorkMock;
    private Mock<IStockNotificationRepository> stockNotificationRepoMock;
    private Mock<IClothesStockRepository> clothesStockRepoMock;
    private Mock<IUserClaimsExtractor> userClaimsExtractorMock;
    private Mock<IPublishEndpoint> publishEndpointMock;
    private Mock<ILogger<StockNotificationService>> loggerMock;

    private StockNotificationService stockNotificationService;

    public StockNotificationServiceTests()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        stockNotificationRepoMock = new Mock<IStockNotificationRepository>();
        clothesStockRepoMock = new Mock<IClothesStockRepository>();
        userClaimsExtractorMock = new Mock<IUserClaimsExtractor>();
        publishEndpointMock = new Mock<IPublishEndpoint>();
        loggerMock = new Mock<ILogger<StockNotificationService>>();

        unitOfWorkMock.Setup(u => u.StockNotification).Returns(stockNotificationRepoMock.Object);
        unitOfWorkMock.Setup(u => u.ClothesStocks).Returns(clothesStockRepoMock.Object);

        stockNotificationService = new StockNotificationService(
            unitOfWorkMock.Object,
            userClaimsExtractorMock.Object,
            publishEndpointMock.Object,
            loggerMock.Object);
    }

    private StockNotification BuildStockNotification(Guid stockId)
    {
        Guid clotheId = Guid.NewGuid();
        return new StockNotification
        {
            Id = Guid.NewGuid(),
            StockId = stockId,
            UserId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            UserFirstName = "John",
            IsNotified = false,
            Stock = new ClothesStock
            {
                Id = stockId,
                Clothe = new ClotheItem { Id = clotheId, Name = "Test Clothe", Slug = "test-clothe" },
                Size = new Size { Name = "M" },
                Color = new Color { Name = "Red", Slug = "red" }
            }
        };
    }
    
    [Fact]
    public async Task NotifySubscribersAsync_WhenSubscribersExist_PublishesEventForEach()
    {
        Guid stockId = Guid.NewGuid();
        List<StockNotification> notifications = new List<StockNotification>
        {
            BuildStockNotification(stockId),
            BuildStockNotification(stockId),
            BuildStockNotification(stockId)
        };

        stockNotificationRepoMock
            .Setup(r => r.GetAllSubscribersByStockId(stockId, default))
            .ReturnsAsync(notifications);

        await stockNotificationService.NotifySubscribersAsync(stockId);

        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ClotheStockUpdatedEvent>(), default),
            Times.Exactly(3));
    }

    [Fact]
    public async Task NotifySubscribersAsync_WhenSubscribersExist_SetsIsNotifiedTrueAndSaves()
    {
        Guid stockId = Guid.NewGuid();
        StockNotification notification = BuildStockNotification(stockId);

        stockNotificationRepoMock
            .Setup(r => r.GetAllSubscribersByStockId(stockId, default))
            .ReturnsAsync(new List<StockNotification> { notification });

        await stockNotificationService.NotifySubscribersAsync(stockId);

        notification.IsNotified.Should().BeTrue();
        stockNotificationRepoMock.Verify(r => r.Update(notification), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribersAsync_WhenSubscribersExist_PublishesCorrectEventData()
    {
        Guid stockId = Guid.NewGuid();
        StockNotification notification = BuildStockNotification(stockId);

        stockNotificationRepoMock
            .Setup(r => r.GetAllSubscribersByStockId(stockId, default))
            .ReturnsAsync(new List<StockNotification> { notification });

        await stockNotificationService.NotifySubscribersAsync(stockId);

        publishEndpointMock.Verify(p => p.Publish(
            It.Is<ClotheStockUpdatedEvent>(e =>
                e.UserEmail == notification.UserEmail &&
                e.UserFirstName == notification.UserFirstName &&
                e.ClotheId == notification.Stock!.Clothe!.Id &&
                e.Size == notification.Stock!.Size!.Name &&
                e.Color == notification.Stock!.Color!.Name),
            default), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribersAsync_WhenNoSubscribers_DoesNotPublishOrSave()
    {
        Guid stockId = Guid.NewGuid();

        stockNotificationRepoMock
            .Setup(r => r.GetAllSubscribersByStockId(stockId, default))
            .ReturnsAsync(new List<StockNotification>());

        await stockNotificationService.NotifySubscribersAsync(stockId);

        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<ClotheStockUpdatedEvent>(), default),
            Times.Never);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task SubscribeForStockAsync_WhenStockNotFound_ThrowsNotFoundException()
    {
        Guid stockId = Guid.NewGuid();
        ClaimsPrincipal claims = new ClaimsPrincipal();

        clothesStockRepoMock
            .Setup(r => r.GetByIdAsync(stockId, default))
            .ReturnsAsync((ClothesStock?)null);

        Func<Task> act = async () => await stockNotificationService.SubscribeForStockAsync(stockId, claims);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{stockId}*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task SubscribeForStockAsync_WhenStockQuantityIsPositive_ThrowsValidationFailedException()
    {
        Guid stockId = Guid.NewGuid();
        ClaimsPrincipal claims = new ClaimsPrincipal();

        clothesStockRepoMock
            .Setup(r => r.GetByIdAsync(stockId, default))
            .ReturnsAsync(new ClothesStock { Id = stockId, Quantity = 5 });

        Func<Task> act = async () => await stockNotificationService.SubscribeForStockAsync(stockId, claims);

        await act.Should().ThrowAsync<ValidationFailedException>().WithMessage("*in stock*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task SubscribeForStockAsync_WhenAlreadySubscribed_ThrowsAlreadyExistsException()
    {
        Guid stockId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = new ClaimsPrincipal();

        clothesStockRepoMock
            .Setup(r => r.GetByIdAsync(stockId, default))
            .ReturnsAsync(new ClothesStock { Id = stockId, Quantity = 0 });

        userClaimsExtractorMock
            .Setup(e => e.GetUserId(claims))
            .Returns(userId);

        stockNotificationRepoMock
            .Setup(r => r.HasUserAlreadySubscribeInStockId(userId, stockId, default))
            .ReturnsAsync(true);

        Func<Task> act = async () => await stockNotificationService.SubscribeForStockAsync(stockId, claims);

        await act.Should().ThrowAsync<AlreadyExistsException>().WithMessage("*already subscribed*");
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task SubscribeForStockAsync_WhenValidData_CreatesNotificationAndSaves()
    {
        Guid stockId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = new ClaimsPrincipal();

        clothesStockRepoMock
            .Setup(r => r.GetByIdAsync(stockId, default))
            .ReturnsAsync(new ClothesStock { Id = stockId, Quantity = 0 });

        userClaimsExtractorMock.Setup(e => e.GetUserId(claims)).Returns(userId);
        userClaimsExtractorMock.Setup(e => e.GetEmail(claims)).Returns("user@example.com");
        userClaimsExtractorMock.Setup(e => e.GetFirstName(claims)).Returns("Jane");

        stockNotificationRepoMock
            .Setup(r => r.HasUserAlreadySubscribeInStockId(userId, stockId, default))
            .ReturnsAsync(false);

        await stockNotificationService.SubscribeForStockAsync(stockId, claims);

        stockNotificationRepoMock.Verify(r => r.AddAsync(
            It.Is<StockNotification>(n =>
                n.StockId == stockId &&
                n.UserId == userId &&
                n.UserEmail == "user@example.com" &&
                n.UserFirstName == "Jane" &&
                n.IsNotified == false),
            default), Times.Once);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}