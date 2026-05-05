using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.Entities.Stock;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class ClothesStockControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public ClothesStockControllerTests(CatalogServiceWebApplicationFactory factory)
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
        db.Brands.RemoveRange(db.Brands);
        db.ClothingTypes.RemoveRange(db.ClothingTypes);
        db.Collections.RemoveRange(db.Collections);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPaged_ShouldReturnUnauthorized_WhenNoToken()
    {
        HttpResponseMessage response = await client.GetAsync("/api/clothes-stock");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPaged_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        HttpResponseMessage response = await client.GetAsync("/api/clothes-stock");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_AsManager_ShouldReturnOk()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        HttpResponseMessage response = await client.GetAsync("/api/clothes-stock");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPaged_AsAdmin_ShouldReturnOk()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.GetAsync("/api/clothes-stock");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenNoToken()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/clothes-stock/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        (Guid stockId, _, _, _) = await SeedStockAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.GetAsync($"/api/clothes-stock/{stockId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ClothesStockReadDTO? result = await response.Content.ReadFromJsonAsync<ClothesStockReadDTO>();
        result!.Id.Should().Be(stockId);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        HttpResponseMessage response = await client.GetAsync($"/api/clothes-stock/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        (_, Guid clotheId, Guid colorId, Guid sizeId) = await SeedStockAsync();

        ClothesStockCreateDTO dto = new ClothesStockCreateDTO
        {
            ClotheId = clotheId,
            ColorId = colorId,
            SizeId = sizeId,
            Quantity = 5
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothes-stock", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        (_, Guid clotheId, Guid colorId, Guid sizeId) = await SeedDependenciesOnlyAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });

        ClothesStockCreateDTO dto = new ClothesStockCreateDTO
        {
            ClotheId = clotheId,
            ColorId = colorId,
            SizeId = sizeId,
            Quantity = 5
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothes-stock", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        (_, Guid clotheId, Guid colorId, Guid sizeId) = await SeedDependenciesOnlyAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        ClothesStockCreateDTO dto = new ClothesStockCreateDTO
        {
            ClotheId = clotheId,
            ColorId = colorId,
            SizeId = sizeId,
            Quantity = 10
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothes-stock", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ClothesStockReadDTO? result = await response.Content.ReadFromJsonAsync<ClothesStockReadDTO>();
        result!.Quantity.Should().Be(10);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenStockAlreadyExists()
    {
        (_, Guid clotheId, Guid colorId, Guid sizeId) = await SeedStockAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        ClothesStockCreateDTO dto = new ClothesStockCreateDTO
        {
            ClotheId = clotheId,
            ColorId = colorId,
            SizeId = sizeId,
            Quantity = 5
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothes-stock", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenClotheDoesNotExist()
    {
        (_, _, Guid colorId, Guid sizeId) = await SeedDependenciesOnlyAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        ClothesStockCreateDTO dto = new ClothesStockCreateDTO
        {
            ClotheId = Guid.NewGuid(),
            ColorId = colorId,
            SizeId = sizeId,
            Quantity = 5
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothes-stock", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        (Guid stockId, _, _, _) = await SeedStockAsync();
        ClothesStockUpdateDTO dto = new ClothesStockUpdateDTO { Quantity = 99 };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothes-stock/{stockId}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkWithUpdatedQuantity()
    {
        (Guid stockId, _, _, _) = await SeedStockAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        ClothesStockUpdateDTO dto = new ClothesStockUpdateDTO { Quantity = 42 };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothes-stock/{stockId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ClothesStockReadDTO? result = await response.Content.ReadFromJsonAsync<ClothesStockReadDTO>();
        result!.Quantity.Should().Be(42);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ClothesStockUpdateDTO dto = new ClothesStockUpdateDTO { Quantity = 5 };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothes-stock/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        (Guid stockId, _, _, _) = await SeedStockAsync();
        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes-stock/{stockId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnNoContent()
    {
        (Guid stockId, _, _, _) = await SeedStockAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes-stock/{stockId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes-stock/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsUser_ShouldReturnForbidden()
    {
        (Guid stockId, _, _, _) = await SeedStockAsync();
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothes-stock/{stockId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<(Guid stockId, Guid clotheId, Guid colorId, Guid sizeId)> SeedStockAsync(int quantity = 0)
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
            Description = "Test",
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

        return (stock.Id, clothe.Id, color.Id, size.Id);
    }

    private async Task<(Guid stockId, Guid clotheId, Guid colorId, Guid sizeId)> SeedDependenciesOnlyAsync()
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
            Description = "Test",
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

        db.Sizes.Add(size);
        db.Colors.Add(color);
        db.ClotheItems.Add(clothe);
        await db.SaveChangesAsync();

        return (Guid.Empty, clothe.Id, color.Id, size.Id);
    }
}