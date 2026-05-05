using Clothy.ReviewService.Application.Features.Reviews.Commands.DeleteReview;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Commands;

public class DeleteReviewCommandHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private DeleteReviewCommandHandler handler;
    
    public DeleteReviewCommandHandlerTests()
    {
        handler = new DeleteReviewCommandHandler(repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_OwnerDeletes_DeletesSuccessfully()
    {
        Review review = ReviewFakes.CreateReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        
        DeleteReviewCommand command = new DeleteReviewCommand(review.Id, review.User.UserId, false, false);
        await handler.Handle(command, default);
        
        repositoryMock.Verify(r => r.DeleteAsync(review.Id, default), Times.Once);
    }

    [Fact]
    public async Task Handle_AdminDeletes_DeletesSuccessfully()
    {
        Review review = ReviewFakes.CreateReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        
        DeleteReviewCommand command = new DeleteReviewCommand(review.Id, Guid.NewGuid(), IsAdmin: true, IsManager: false);
        await handler.Handle(command, default);
        
        repositoryMock.Verify(r => r.DeleteAsync(review.Id, default), Times.Once);
    }

    [Fact]
    public async Task Handle_ManagerDeletes_DeletesSuccessfully()
    {
        Review review = ReviewFakes.CreateReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
        
        DeleteReviewCommand command = new DeleteReviewCommand(review.Id, Guid.NewGuid(), IsAdmin: false, IsManager: true);
        await handler.Handle(command, default);
        
        repositoryMock.Verify(r => r.DeleteAsync(review.Id, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_OtherUserDeletes_ThrowsForbiddenException()
    {
        Review review = ReviewFakes.CreateReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        DeleteReviewCommand command = new DeleteReviewCommand(review.Id, Guid.NewGuid(), false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<ForbiddenException>();
        repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>(), default), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Review?)null);
 
        DeleteReviewCommand command = new DeleteReviewCommand("nonexistent-id", Guid.NewGuid(), false, false);
        var act = () => handler.Handle(command, default);
 
        await act.Should().ThrowAsync<NotFoundException>();
    }
}