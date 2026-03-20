using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewStatistics;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Query;

public class GetReviewStatisticsQueryHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private Mock<IClotheItemIdValidatorGrpcClient> clotheValidatorMock = new();
    private GetReviewStatisticsQueryHandler handler;
 
    private static Guid _clotheId = Guid.NewGuid();
 
    public GetReviewStatisticsQueryHandlerTests()
    {
        handler = new GetReviewStatisticsQueryHandler(
            repositoryMock.Object,
            clotheValidatorMock.Object
        );
    }
 
    private void SetupClotheValidatorValid() =>
        clotheValidatorMock
            .Setup(c => c.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(x => x.ClotheId == _clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = true });
 
    private void SetupClotheValidatorInvalid() =>
        clotheValidatorMock
            .Setup(c => c.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(x => x.ClotheId == _clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = false, ErrorMessage = "Clothe not found" });
 
    private static ReviewStatistics BuildStatistics() => new()
    {
        TotalReviews = 10,
        FiveStars = 5,
        FourStars = 3,
        ThreeStars = 1,
        TwoStars = 1,
        OneStar = 0,
        AverageRating = 4.3
    };
 
    [Fact]
    public async Task Handle_ValidClotheId_ReturnsStatistics()
    {
        ReviewStatistics expected = BuildStatistics();
        SetupClotheValidatorValid();
        repositoryMock.Setup(r => r.ClotheItemExistsAsync(_clotheId, default)).ReturnsAsync(true);
        repositoryMock.Setup(r => r.GetReviewStatisticsAsync(_clotheId, default)).ReturnsAsync(expected);
 
        ReviewStatistics result = await handler.Handle(new GetReviewStatisticsQuery(_clotheId), default);
 
        result.Should().BeEquivalentTo(expected);
    }
 
    [Fact]
    public async Task Handle_InvalidClotheId_ThrowsValidationFailedException()
    {
        SetupClotheValidatorInvalid();
 
        var act = () => handler.Handle(new GetReviewStatisticsQuery(_clotheId), default);
 
        await act.Should().ThrowAsync<ValidationFailedException>();
        repositoryMock.Verify(r => r.GetReviewStatisticsAsync(It.IsAny<Guid>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_ClotheItemNotInRepository_ThrowsNotFoundException()
    {
        SetupClotheValidatorValid();
        repositoryMock.Setup(r => r.ClotheItemExistsAsync(_clotheId, default)).ReturnsAsync(false);
 
        var act = () => handler.Handle(new GetReviewStatisticsQuery(_clotheId), default);
 
        await act.Should().ThrowAsync<NotFoundException>();
        repositoryMock.Verify(r => r.GetReviewStatisticsAsync(It.IsAny<Guid>(), default), Times.Never);
    }
 
    [Fact]
    public async Task Handle_ValidRequest_CallsRepositoryWithCorrectClotheId()
    {
        SetupClotheValidatorValid();
        repositoryMock.Setup(r => r.ClotheItemExistsAsync(_clotheId, default)).ReturnsAsync(true);
        repositoryMock.Setup(r => r.GetReviewStatisticsAsync(_clotheId, default)).ReturnsAsync(BuildStatistics());
 
        await handler.Handle(new GetReviewStatisticsQuery(_clotheId), default);
 
        repositoryMock.Verify(r => r.GetReviewStatisticsAsync(_clotheId, default), Times.Once);
    }
}