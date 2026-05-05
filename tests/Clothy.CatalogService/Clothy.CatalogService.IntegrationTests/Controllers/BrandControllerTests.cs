using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class BrandControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public BrandControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.Brands.RemoveRange(db.Brands);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoBrands()
    {
        HttpResponseMessage response = await client.GetAsync("/api/brands");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BrandReadDTO>? result = await response.Content.ReadFromJsonAsync<List<BrandReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenBrandExists()
    {
        Guid brandId = await SeedBrandAsync("Adidas", "adidas");

        HttpResponseMessage response = await client.GetAsync($"/api/brands/{brandId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        BrandReadDTO? result = await response.Content.ReadFromJsonAsync<BrandReadDTO>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(brandId);
        result.Name.Should().Be("Adidas");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenBrandDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/brands/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        BrandCreateDTO dto = new BrandCreateDTO
        {
            Name = "Puma",
            Slug = "puma"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/brands", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        BrandCreateDTO dto = new BrandCreateDTO
        {
            Name = "Puma",
            Slug = "puma"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/brands", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        BrandCreateDTO dto = new BrandCreateDTO
        {
            Name = "Reebok",
            Slug = "reebok"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/brands", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<BrandReadDTO>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Reebok");
        result.Slug.Should().Be("reebok");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        BrandCreateDTO dto = new BrandCreateDTO
        {
            Name = "NewBalance",
            Slug = "new-balance"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/brands", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedBrandAsync("Converse", "converse");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        BrandCreateDTO dto = new BrandCreateDTO
        {
            Name = "Converse",
            Slug = "converse-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/brands", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        await SeedBrandAsync("Vans", "vans");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        BrandCreateDTO dto = new BrandCreateDTO
        {
            Name = "Vans 2",
            Slug = "vans"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/brands", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid brandId = await SeedBrandAsync("OldBrand", "old-brand");
        BrandUpdateDTO dto = new BrandUpdateDTO
        {
            Name = "NewBrand",
            Slug = "new-brand"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/brands/{brandId}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedData()
    {
        Guid brandId = await SeedBrandAsync("BrandToUpdate", "brand-to-update");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        BrandUpdateDTO dto = new BrandUpdateDTO
        {
            Name = "UpdatedBrand",
            Slug = "updated-brand"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/brands/{brandId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BrandReadDTO>();
        result!.Name.Should().Be("UpdatedBrand");
        result.Slug.Should().Be("updated-brand");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenBrandDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        BrandUpdateDTO dto = new BrandUpdateDTO
        {
            Name = "Ghost",
            Slug = "ghost"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/brands/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenNameTakenByAnotherBrand()
    {
        await SeedBrandAsync("ExistingBrand", "existing-brand");
        Guid brandId = await SeedBrandAsync("BrandToEdit", "brand-to-edit");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        BrandUpdateDTO dto = new BrandUpdateDTO
        {
            Name = "ExistingBrand",
            Slug = "brand-to-edit"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/brands/{brandId}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid brandId = await SeedBrandAsync("ToDelete", "to-delete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/brands/{brandId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid brandId = await SeedBrandAsync("ToDeleteManager", "to-delete-manager");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/brands/{brandId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid brandId = await SeedBrandAsync("ToDeleteAdmin", "to-delete-admin");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/brands/{brandId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenBrandDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/brands/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedBrandAsync(string name, string slug)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        Brand brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        db.Brands.Add(brand);
        await db.SaveChangesAsync();

        return brand.Id;
    }
}