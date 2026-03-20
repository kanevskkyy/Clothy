using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviews;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.UnitTests.Helpers;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Features.Reviews.Query;

public class GetReviewsQueryHandlerTests
{
    private Mock<IReviewRepository> repositoryMock = new();
    private GetReviewsQueryHandler handler;
 
    public GetReviewsQueryHandlerTests()
    {
        handler = new GetReviewsQueryHandler(repositoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_RegularUser_ForcesConfirmedStatus()
    {
        ReviewQueryParameters queryParams = new ReviewQueryParameters();
        GetReviewsQuery query = new GetReviewsQuery(queryParams, userId: null, isAdmin: false, isManager: false);
        
        PagedList<Review> expected = new PagedList<Review>([], 1, 10, 0);
        repositoryMock.Setup(r => r.GetReviewsAsync(queryParams, default)).ReturnsAsync(expected);
 
        await handler.Handle(query, default);
 
        queryParams.Status.Should().Be(ReviewStatus.Confirmed);
    }
    
    [Fact]
    public async Task Handle_Admin_DoesNotForceConfirmedStatus()
    {
        ReviewQueryParameters queryParams = new ReviewQueryParameters { Status = ReviewStatus.Pending };
        GetReviewsQuery query = new GetReviewsQuery(queryParams, isAdmin: true);
        PagedList<Review> expected = new PagedList<Review>([], 1, 10, 0);
        
        repositoryMock.Setup(r => r.GetReviewsAsync(queryParams, default)).ReturnsAsync(expected);
        await handler.Handle(query, default);
 
        queryParams.Status.Should().Be(ReviewStatus.Pending);
    }
 
    [Fact]
    public async Task Handle_Manager_DoesNotForceConfirmedStatus()
    {
        ReviewQueryParameters queryParams = new ReviewQueryParameters { Status = ReviewStatus.Pending };
        GetReviewsQuery query = new GetReviewsQuery(queryParams, isManager: true);
        
        PagedList<Review> expected = new PagedList<Review>([], 1, 10, 0);
        repositoryMock.Setup(r => r.GetReviewsAsync(queryParams, default)).ReturnsAsync(expected);
 
        await handler.Handle(query, default);
 
        queryParams.Status.Should().Be(ReviewStatus.Pending);
    }
 
    [Fact]
    public async Task Handle_ReturnsPagedListFromRepository()
    {
        ReviewQueryParameters queryParams = new ReviewQueryParameters();
        GetReviewsQuery query = new GetReviewsQuery(queryParams, isAdmin: true);
        
        PagedList<Review> expected = new PagedList<Review>([ReviewFakes.CreateConfirmedReview()], 1, 10, 1);
        repositoryMock.Setup(r => r.GetReviewsAsync(queryParams, default)).ReturnsAsync(expected);
 
        PagedList<Review> result = await handler.Handle(query, default);
 
        result.Should().BeEquivalentTo(expected);
    }
}