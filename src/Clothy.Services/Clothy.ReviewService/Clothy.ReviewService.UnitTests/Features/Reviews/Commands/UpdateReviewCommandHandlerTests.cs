using Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Commands;

public class UpdateReviewCommandHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private UpdateReviewWithIdCommandHandler handler;
 
    public UpdateReviewCommandHandlerTests()
    {
        handler = new UpdateReviewWithIdCommandHandler(repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_OwnerUpdates_UpdatesSuccessfully()
    {
        Review review = ReviewFakes.CreateReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        UpdateReviewWithIdCommand command = new UpdateReviewWithIdCommand(review.Id, "Updated comment", 4, review.User.UserId);
        await handler.Handle(command, default);
 
        review.Comment.Should().Be("Updated comment");
        review.Rating.Should().Be(4);
        repositoryMock.Verify(r => r.UpdateAsync(review, default), Times.Once);
    }

    [Fact]
    public async Task Handle_OtherUserUpdates_ThrowsForbiddenException()
    {
        Review review = ReviewFakes.CreateReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);

        UpdateReviewWithIdCommand command = new UpdateReviewWithIdCommand(review.Id, "Hacked comment", 1, Guid.NewGuid());
        var act = () => handler.Handle(command, default);
        
        await act.Should().ThrowAsync<ForbiddenException>();
        repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Review>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Review?)null);
        
        UpdateReviewWithIdCommand command = new UpdateReviewWithIdCommand("nonexistent-id", "comment", 3, Guid.NewGuid());
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}