using System.Security.Claims;
using Clothy.ReviewService.API.Controllers;
using Clothy.ReviewService.Application.Features.Reviews.Commands.ConfirmReview;
using Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview;
using Clothy.ReviewService.Application.Features.Reviews.Commands.DeleteReview;
using Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview;
using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewById;
using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviews;
using Clothy.ReviewService.Application.Features.Reviews.Query.GetReviewStatistics;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Clothy.ReviewService.UnitTests.Controllers;

public class ReviewsControllerTests
{
    private Mock<IMediator> mediatorMock;
    private Mock<ILogger<ReviewsController>> loggerMock;
    private Mock<IUserClaimsExtractor> claimsExtractorMock;
    private ReviewsController sut;

    private Guid USER_ID = Guid.NewGuid();
    private const string FIRST_NAME = "John";
    private const string LAST_NAME = "Doe";
    private const string PHOTO_URL = "https://example.com/photo.jpg";

    public ReviewsControllerTests()
    {
        mediatorMock = new Mock<IMediator>();
        loggerMock = new Mock<ILogger<ReviewsController>>();
        claimsExtractorMock = new Mock<IUserClaimsExtractor>();

        sut = new ReviewsController(
            mediatorMock.Object,
            loggerMock.Object,
            claimsExtractorMock.Object);

        SetUser(isAuthenticated: true, isAdmin: false, isManager: false);

        claimsExtractorMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(USER_ID);
        claimsExtractorMock.Setup(x => x.GetFirstName(It.IsAny<ClaimsPrincipal>())).Returns(FIRST_NAME);
        claimsExtractorMock.Setup(x => x.GetLastName(It.IsAny<ClaimsPrincipal>())).Returns(LAST_NAME);
        claimsExtractorMock.Setup(x => x.GetPhotoUrl(It.IsAny<ClaimsPrincipal>())).Returns(PHOTO_URL);
    }
    
    [Fact]
    public async Task GetReviews_AuthenticatedRegularUser_SendsQueryWithUserIdAndNoRoles()
    {
        ReviewQueryParameters queryParams = new ReviewQueryParameters
        {
            PageNumber = 1,
            PageSize = 10
        };
        PagedList<Review> pagedList = new PagedList<Review>(new List<Review>(), 0, 1, 10);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        IActionResult result = await sut.GetReviews(queryParams, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(pagedList);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewsQuery>(q =>
                q.QueryParameters == queryParams &&
                q.UserId == USER_ID &&
                q.IsAdmin == false &&
                q.IsManager == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReviews_AnonymousUser_SendsQueryWithNullUserIdAndNoRoles()
    {
        SetUser(isAuthenticated: false);
        var queryParams = new ReviewQueryParameters { PageNumber = 1, PageSize = 5 };
        var pagedList = new PagedList<Review>(new List<Review>(), 0, 1, 5);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        IActionResult result = await sut.GetReviews(queryParams, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewsQuery>(q =>
                q.UserId == null &&
                q.IsAdmin == false &&
                q.IsManager == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReviews_AdminUser_SendsQueryWithIsAdminTrue()
    {
        SetUser(isAuthenticated: true, isAdmin: true);
        ReviewQueryParameters queryParams = new ReviewQueryParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Review>(new List<Review>(), 0, 1, 10));

        await sut.GetReviews(queryParams, CancellationToken.None);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewsQuery>(q => q.IsAdmin == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReviews_ManagerUser_SendsQueryWithIsManagerTrue()
    {
        SetUser(isAuthenticated: true, isManager: true);
        ReviewQueryParameters queryParams = new ReviewQueryParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Review>(new List<Review>(), 0, 1, 10));

        await sut.GetReviews(queryParams, CancellationToken.None);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewsQuery>(q => q.IsManager == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetReviewById_AnonymousUser_SendsQueryWithNoRoles()
    {
        SetUser(isAuthenticated: false);
        Review review = BuildReview();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        IActionResult result = await sut.GetReviewById(review.Id, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(review);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewByIdQuery>(q =>
                q.ReviewId == review.Id &&
                q.IsAdmin == false &&
                q.IsManager == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReviewById_AdminUser_SendsQueryWithIsAdminTrue()
    {
        SetUser(isAuthenticated: true, isAdmin: true);
        Review review = BuildReview();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        await sut.GetReviewById(review.Id, CancellationToken.None);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewByIdQuery>(q => q.IsAdmin == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReviewById_ManagerUser_SendsQueryWithIsManagerTrue()
    {
        SetUser(isAuthenticated: true, isManager: true);
        Review review = BuildReview();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        await sut.GetReviewById(review.Id, CancellationToken.None);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewByIdQuery>(q => q.IsManager == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateReview_ReturnsCreatedAtAction_WithReview()
    {
        CreateReviewDTO dto = new CreateReviewDTO
        {
            ClotheItemId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great shirt!"
        };
        Review review = BuildReview();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        IActionResult result = await sut.CreateReview(dto, CancellationToken.None);

        CreatedAtActionResult created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(sut.GetReviewById));
        created.RouteValues!["id"].Should().Be(review.Id);
        created.Value.Should().Be(review);

        mediatorMock.Verify(m => m.Send(
            It.Is<CreateReviewCommand>(c =>
                c.ClotheItemId == dto.ClotheItemId &&
                c.Rating == dto.Rating &&
                c.Comment == dto.Comment &&
                c.UserId == USER_ID &&
                c.FirstName == FIRST_NAME &&
                c.LastName == LAST_NAME &&
                c.PhotoUrl == PHOTO_URL),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReviewStatistics_ReturnsOk_WithStatistics()
    {
        Guid clotheItemId = Guid.NewGuid();
        ReviewStatistics stats = new ReviewStatistics
        {
            TotalReviews = 10,
            AverageRating = 4.5,
            FiveStars = 5,
            FourStars = 3,
            ThreeStars = 1,
            TwoStars = 1,
            OneStar = 0
        };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetReviewStatisticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        IActionResult result = await sut.GetReviewStatistics(clotheItemId, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(stats);

        mediatorMock.Verify(m => m.Send(
            It.Is<GetReviewStatisticsQuery>(q => q.ClotheItemId == clotheItemId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task ConfirmReview_ReturnsOk_WithConfirmedReview()
    {
        const string reviewId = "r-001";
        Review review = BuildReview();

        mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        IActionResult result = await sut.ConfirmReview(reviewId, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(review);

        mediatorMock.Verify(m => m.Send(
            It.Is<ConfirmReviewCommand>(c => c.ReviewId == reviewId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteReview_AsRegularUser_ReturnsNoContent()
    {
        const string reviewId = "r-001";

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.DeleteReview(reviewId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteReviewCommand>(c =>
                c.ReviewId == reviewId &&
                c.UserId == USER_ID &&
                c.IsAdmin == false &&
                c.IsManager == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_AsAdmin_PassesIsAdminTrue()
    {
        SetUser(isAuthenticated: true, isAdmin: true);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.DeleteReview("r-001", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteReviewCommand>(c => c.IsAdmin == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_AsManager_PassesIsManagerTrue()
    {
        SetUser(isAuthenticated: true, isManager: true);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        IActionResult result = await sut.DeleteReview("r-001", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        mediatorMock.Verify(m => m.Send(
            It.Is<DeleteReviewCommand>(c => c.IsManager == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetUser(bool isAuthenticated, bool isAdmin = false, bool isManager = false)
    {
        ClaimsIdentity identity = isAuthenticated
            ? new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }, "TestAuth")
            : new ClaimsIdentity();

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        claimsExtractorMock
            .Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Admin"))
            .Returns(isAdmin);

        claimsExtractorMock
            .Setup(x => x.IsInRole(It.IsAny<ClaimsPrincipal>(), "Manager"))
            .Returns(isManager);
    }

    private Review BuildReview()
    {
        ClotheInfo clotheInfo = new ClotheInfo(Guid.NewGuid(), "Cool T-Shirt", "https://example.com/shirt.jpg");
        UserInfo userInfo = new UserInfo(USER_ID, FIRST_NAME, LAST_NAME, PHOTO_URL);
        
        return new Review(clotheInfo, userInfo, 5, "Great product!");
    }
}