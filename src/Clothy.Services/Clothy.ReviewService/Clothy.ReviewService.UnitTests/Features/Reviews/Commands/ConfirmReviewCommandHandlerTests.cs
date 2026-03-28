using Clothy.ReviewService.Application.Features.Reviews.Commands.ConfirmReview;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Commands;

public class ConfirmReviewCommandHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private ConfirmReviewCommandHandler handler;
 
    public ConfirmReviewCommandHandlerTests()
    {
        handler = new ConfirmReviewCommandHandler(repositoryMock.Object);
    }
 
    [Fact]
    public async Task Handle_ReviewExists_ConfirmsAndReturnsReview()
    {
        Review review = ReviewFakes.CreatePendingReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        Review result = await handler.Handle(new ConfirmReviewCommand(review.Id), default);
 
        result.Status.Should().Be(ReviewStatus.Confirmed);
        repositoryMock.Verify(r => r.UpdateAsync(review, default), Times.Once);
    }
 
    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Review?)null);
 
        var act = () => handler.Handle(new ConfirmReviewCommand("nonexistent-id"), default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}