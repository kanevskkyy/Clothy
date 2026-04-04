using System.Net;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

public class StockNotificationControllerTests : IClassFixture<CatalogServiceWebApplicationFactory>, IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public StockNotificationControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        db.StockNotifications.RemoveRange(db.StockNotifications);
        db.ClothesStocks.RemoveRange(db.ClothesStocks);
        db.Colors.RemoveRange(db.Colors);
        db.Sizes.RemoveRange(db.Sizes);
        db.ClotheItems.RemoveRange(db.ClotheItems);
        await db.SaveChangesAsync();
    }
    
    [Fact]
    public async Task Subscribe_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid stockId = await SeedOutOfStockAsync();

        HttpResponseMessage response = await client.PostAsync(
            $"/api/stocks/notifications/subscribe/{stockId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Subscribe_AsUser_ShouldReturnNoContent_WhenStockIsEmpty()
    {
        Guid stockId = await SeedOutOfStockAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });

        HttpResponseMessage response = await client.PostAsync(
            $"/api/stocks/notifications/subscribe/{stockId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Subscribe_ShouldReturnNotFound_WhenStockDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });

        HttpResponseMessage response = await client.PostAsync(
            $"/api/stocks/notifications/subscribe/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Subscribe_ShouldReturnConflict_WhenAlreadySubscribed()
    {
        Guid userId = Guid.NewGuid();
        Guid stockId = await SeedOutOfStockAsync();

        client.AddAuthorizationHeader(userId, new[] { "User" });
        await client.PostAsync($"/api/stocks/notifications/subscribe/{stockId}", null);

        client.AddAuthorizationHeader(userId, new[] { "User" });
        HttpResponseMessage response = await client.PostAsync(
            $"/api/stocks/notifications/subscribe/{stockId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    private async Task<Guid> SeedOutOfStockAsync() => await SeedStockAsync(quantity: 0);
    private async Task<Guid> SeedInStockAsync() => await SeedStockAsync(quantity: 10);
    
    private async Task<Guid> SeedStockAsync(int quantity)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        string u = Guid.NewGuid().ToString("N")[..8];

        Size size = new Size
        {
            Id = Guid.NewGuid(),
            Name = $"M-{u}"
        };
        Color color = new Color
        {
            Id = Guid.NewGuid(),
            Name = $"Black-{u}",
            HexCode = "#000000",
            Slug = $"black-{u}"
        };
        ClotheItem clothe = new ClotheItem
        {
            Id = Guid.NewGuid(),
            Name = $"Clothe-{u}",
            Slug = $"clothe-{u}",
            CreatedAt = DateTime.UtcNow,
            Description = "Test Description",
            Price = 100m,
            Gender = Gender.Male,
            ClothyType = new ClothingType
            {
                Id = Guid.NewGuid(),
                Name = $"Type-{u}",
                Slug = $"type-{u}"
            },
            Brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = $"Brand-{u}",
                Slug = $"brand-{u}"
            },
            Collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = $"Col-{u}",
                Slug = $"col-{u}"
            },
        };

        ClothesStock stock = new ClothesStock
        {
            Id = Guid.NewGuid(),
            Clothe = clothe,
            Size = size,
            Color = color,
            Quantity = quantity
        };

        db.Sizes.Add(size);
        db.Colors.Add(color);
        db.ClotheItems.Add(clothe);
        db.ClothesStocks.Add(stock);
        await db.SaveChangesAsync();

        return stock.Id;
    }
}