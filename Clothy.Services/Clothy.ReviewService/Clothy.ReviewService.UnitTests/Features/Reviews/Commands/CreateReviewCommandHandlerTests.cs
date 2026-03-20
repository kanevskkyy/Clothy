using System.Diagnostics.Metrics;
using Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Commands;

public class CreateReviewCommandHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private Mock<IClotheItemIdValidatorGrpcClient> clotheValidatorMock = new();
    private Mock<ICheckUserPurchasedClotheGrpcClient> purchasedMock = new();
    private CreateReviewCommandHandler handler;
 
    private static Guid userId = Guid.NewGuid();
    private static Guid clotheId = Guid.NewGuid();
 
    public CreateReviewCommandHandlerTests()
    {
        Meter meter = new Meter("Clothy.ReviewService.Tests");
 
        handler = new CreateReviewCommandHandler(
            repositoryMock.Object,
            clotheValidatorMock.Object,
            meter,
            purchasedMock.Object
        );
    }
    
    private CreateReviewCommand BuildCommand(Guid? userId = null, Guid? clotheId = null) =>
        new(
            clotheId ?? CreateReviewCommandHandlerTests.clotheId,
            Rating: 5,
            Comment: "Great product!",
            UserId: userId ?? CreateReviewCommandHandlerTests.userId,
            FirstName: "John",
            LastName: "Doe",
            PhotoUrl: "https://example.com/photo.jpg"
        );
 
    private void SetupClotheValidatorValid(Guid clotheId) =>
        clotheValidatorMock
            .Setup(c => c.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(x => x.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = true });
 
    private void SetupClotheValidatorInvalid(Guid clotheId) =>
        clotheValidatorMock
            .Setup(c => c.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(x => x.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = false, ErrorMessage = "Not found" });
 
    private void SetupUserPurchased(Guid userId, Guid clotheId, bool purchased) =>
        purchasedMock
            .Setup(p => p.CheckUserPurchasedAsync(It.Is<CheckUserPurchasedRequest>(
                x => x.UserId == userId.ToString() && x.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new CheckUserPurchasedResponse
            {
                Purchased = purchased,
                ClotheName = "Test Hoodie",
                ClothePhotoURL = "https://example.com/hoodie.jpg"
            });
 
    [Fact]
    public async Task Handle_ValidRequest_CreatesAndReturnsReview()
    {
        SetupClotheValidatorValid(clotheId);
        SetupUserPurchased(userId, clotheId, purchased: true);
        repositoryMock.Setup(r => r.HasUserReviewedClotheAsync(userId, clotheId, default)).ReturnsAsync(false);
 
        Review? result = await handler.Handle(BuildCommand(), default);
 
        result.Should().NotBeNull();
        result.User.UserId.Should().Be(userId);
        result.ClotheInfo.ClotheItemId.Should().Be(clotheId);
        result.Rating.Should().Be(5);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), default), Times.Once);
    }
 
    [Fact]
    public async Task Handle_InvalidClotheId_ThrowsValidationFailedException()
    {
        SetupClotheValidatorInvalid(clotheId);
 
        var act = () => handler.Handle(BuildCommand(), default);
 
        await act.Should().ThrowAsync<ValidationFailedException>();
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_UserDidNotPurchase_ThrowsValidationFailedException()
    {
        SetupClotheValidatorValid(clotheId);
        SetupUserPurchased(userId, clotheId, purchased: false);
 
        var act = () => handler.Handle(BuildCommand(), default);
 
        await act.Should().ThrowAsync<ValidationFailedException>()
            .WithMessage("*did not order*");
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_UserAlreadyReviewed_ThrowsAlreadyExistsException()
    {
        SetupClotheValidatorValid(clotheId);
        SetupUserPurchased(userId, clotheId, purchased: true);
        repositoryMock.Setup(r => r.HasUserReviewedClotheAsync(userId, clotheId, default)).ReturnsAsync(true);
 
        var act = () => handler.Handle(BuildCommand(), default);
 
        await act.Should().ThrowAsync<AlreadyExistsException>();
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_ValidRequest_IncrementsClotheIdMetricTag()
    {
        SetupClotheValidatorValid(clotheId);
        SetupUserPurchased(userId, clotheId, purchased: true);
        repositoryMock.Setup(r => r.HasUserReviewedClotheAsync(userId, clotheId, default)).ReturnsAsync(false);
 
        var act = () => handler.Handle(BuildCommand(), default);
 
        await act.Should().NotThrowAsync();
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), default), Times.Once);
    }
}