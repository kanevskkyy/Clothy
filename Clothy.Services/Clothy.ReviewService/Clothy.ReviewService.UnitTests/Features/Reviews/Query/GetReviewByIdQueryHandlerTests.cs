using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewById;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Query;

public class GetReviewByIdQueryHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private GetReviewByIdQueryHandler handler;
 
    public GetReviewByIdQueryHandlerTests()
    {
        handler = new GetReviewByIdQueryHandler(repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ConfirmedReview_ReturnsReview()
    {
        Review review = ReviewFakes.CreateConfirmedReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        Review? result = await handler.Handle(new GetReviewByIdQuery(review.Id), default);
        result.Should().Be(review);
    }
    
    [Fact]
    public async Task Handle_PendingReview_AsAdmin_ReturnsReview()
    {
        Review review = ReviewFakes.CreatePendingReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        Review? result = await handler.Handle(new GetReviewByIdQuery(review.Id, isAdmin: true), default);
 
        result.Should().Be(review);
    }
    
    [Fact]
    public async Task Handle_PendingReview_AsManager_ReturnsReview()
    {
        Review review = ReviewFakes.CreatePendingReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        Review? result = await handler.Handle(new GetReviewByIdQuery(review.Id, isManager: true), default);
 
        result.Should().Be(review);
    }
 
    [Fact]
    public async Task Handle_PendingReview_AsRegularUser_ThrowsForbiddenException()
    {
        Review review = ReviewFakes.CreatePendingReview();
        repositoryMock.Setup(r => r.GetByIdAsync(review.Id, default)).ReturnsAsync(review);
 
        var act = () => handler.Handle(new GetReviewByIdQuery(review.Id, isAdmin: false, isManager: false), default);
 
        await act.Should().ThrowAsync<ForbiddenException>();
    }
 
    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsNotFoundException()
    {
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default)).ReturnsAsync((Review?)null);
 
        var act = () => handler.Handle(new GetReviewByIdQuery("nonexistent-id"), default);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}