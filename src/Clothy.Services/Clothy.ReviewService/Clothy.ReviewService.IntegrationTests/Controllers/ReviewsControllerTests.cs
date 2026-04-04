using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview;
using Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.ReviewService.Domain.ValueObjects;
using Clothy.ReviewService.IntegrationTests.Infrastructure;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Clothy.ReviewService.IntegrationTests.Controllers;

public class ReviewsControllerTests : IClassFixture<ReviewServiceWebApplicationFactory>
{
    private HttpClient client;
    private ReviewServiceWebApplicationFactory factory;

    private static JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public ReviewsControllerTests(ReviewServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        factory.ClotheItemValidatorMock.Reset();
        factory.CheckUserPurchasedMock.Reset();
    }

    [Fact]
    public async Task GetReviewById_ConfirmedReview_ShouldReturnOk()
    {
        Guid clotheId = Guid.NewGuid();
        Review seeded = await SeedReview(clotheId, Guid.NewGuid(), ReviewStatus.Confirmed);

        HttpResponseMessage response = await client.GetAsync($"/api/reviews/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReviewById_PendingReview_WithoutAuth_ShouldReturnForbidden()
    {
        Guid clotheId = Guid.NewGuid();
        Review seeded = await SeedReview(clotheId, Guid.NewGuid(), ReviewStatus.Pending);

        HttpResponseMessage response = await client.GetAsync($"/api/reviews/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReviewById_PendingReview_AsAdmin_ShouldReturnOk()
    {
        Guid clotheId = Guid.NewGuid();
        Review seeded = await SeedReview(clotheId, Guid.NewGuid(), ReviewStatus.Pending);
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Admin" });

        HttpResponseMessage response = await client.GetAsync($"/api/reviews/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateReview_WithoutAuth_ShouldReturnUnauthorized()
    {
        CreateReviewDTO dto = new()
        {
            ClotheItemId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Nice!"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reviews", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldReturnCreated()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.ClotheItemValidatorMock.SetupValidClotheItem(clotheId);
        factory.CheckUserPurchasedMock.SetupUserPurchased(userId, clotheId);

        CreateReviewDTO dto = new()
        {
            ClotheItemId = clotheId,
            Rating = 4,
            Comment = "Great product!"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reviews", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        Review? result = await response.Content.ReadFromJsonAsync<Review>(JsonOptions);
        result.Should().NotBeNull();
        result!.ClotheInfo.ClotheItemId.Should().Be(clotheId);
        result.User.UserId.Should().Be(userId);
        result.Rating.Should().Be(4);
        result.Status.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public async Task CreateReview_WhenClotheItemInvalid_ShouldReturnBadRequest()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.ClotheItemValidatorMock.SetupInvalidClotheItem(clotheId);

        CreateReviewDTO dto = new()
        {
            ClotheItemId = clotheId,
            Rating = 5,
            Comment = "Test"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reviews", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_WhenUserNotPurchased_ShouldReturnBadRequest()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.ClotheItemValidatorMock.SetupValidClotheItem(clotheId);
        factory.CheckUserPurchasedMock.SetupUserNotPurchased(userId, clotheId);

        CreateReviewDTO dto = new() { ClotheItemId = clotheId, Rating = 5, Comment = "Test" };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reviews", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_WhenAlreadyReviewed_ShouldReturnConflict()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId);

        factory.ClotheItemValidatorMock.SetupValidClotheItem(clotheId);
        factory.CheckUserPurchasedMock.SetupUserPurchased(userId, clotheId);

        CreateReviewDTO dto = new() { ClotheItemId = clotheId, Rating = 5, Comment = "First" };
        await client.PostAsJsonAsync("/api/reviews", dto);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reviews", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateReview_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/reviews/{Guid.NewGuid()}",
            new UpdateReviewDTO("Updated", 3));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateReview_ByOwner_ShouldReturnNoContent()
    {
        Guid userId = Guid.NewGuid();
        Guid clotheId = Guid.NewGuid();
        Review seeded = await SeedReview(clotheId, userId, ReviewStatus.Pending);
        client.AddAuthorizationHeader(userId);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/reviews/{seeded.Id}",
            new UpdateReviewDTO("Updated comment", 2));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateReview_ByAnotherUser_ShouldReturnForbidden()
    {
        Guid owner = Guid.NewGuid();
        Guid attacker = Guid.NewGuid();
        Review seeded = await SeedReview(Guid.NewGuid(), owner, ReviewStatus.Pending);
        client.AddAuthorizationHeader(attacker);

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/reviews/{seeded.Id}",
            new UpdateReviewDTO("Hacked", 1));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConfirmReview_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.PatchAsync(
            $"/api/reviews/status/{Guid.NewGuid()}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConfirmReview_AsRegularUser_ShouldReturnForbidden()
    {
        Review seeded = await SeedReview(Guid.NewGuid(), Guid.NewGuid(), ReviewStatus.Pending);
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "User" });

        HttpResponseMessage response = await client.PatchAsync(
            $"/api/reviews/status/{seeded.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConfirmReview_AsManager_ShouldReturnOk()
    {
        Review seeded = await SeedReview(Guid.NewGuid(), Guid.NewGuid(), ReviewStatus.Pending);
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Manager" });

        HttpResponseMessage response = await client.PatchAsync(
            $"/api/reviews/status/{seeded.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteReview_WithoutAuth_ShouldReturnUnauthorized()
    {
        HttpResponseMessage response = await client.DeleteAsync($"/api/reviews/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteReview_ByOwner_ShouldReturnNoContent()
    {
        Guid userId = Guid.NewGuid();
        Review seeded = await SeedReview(Guid.NewGuid(), userId, ReviewStatus.Pending);
        client.AddAuthorizationHeader(userId);

        HttpResponseMessage response = await client.DeleteAsync($"/api/reviews/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteReview_ByAnotherUser_ShouldReturnForbidden()
    {
        Review seeded = await SeedReview(Guid.NewGuid(), Guid.NewGuid(), ReviewStatus.Pending);
        client.AddAuthorizationHeader(Guid.NewGuid());

        HttpResponseMessage response = await client.DeleteAsync($"/api/reviews/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteReview_ByAdmin_ShouldReturnNoContent()
    {
        Review seeded = await SeedReview(Guid.NewGuid(), Guid.NewGuid(), ReviewStatus.Pending);
        client.AddAuthorizationHeader(Guid.NewGuid(), roles: new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/reviews/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<Review> SeedReview(Guid clotheId, Guid userId, ReviewStatus status)
    {
        using var scope = factory.Services.CreateScope();
        IReviewRepository repo = scope.ServiceProvider.GetRequiredService<IReviewRepository>();

        Review review = new Review(
            new ClotheInfo(clotheId, "Test Clothe", "https://photo.test/img.jpg"),
            new UserInfo(userId, "Test", "User", "https://photo.test/user.jpg"),
            rating: 5,
            comment: "Seeded review"
        );

        if (status == ReviewStatus.Confirmed) review.ConfirmStatus();

        await repo.AddAsync(review, CancellationToken.None);
        return review;
    }
}