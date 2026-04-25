using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class ClothingTypeControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public ClothingTypeControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.ClothingTypes.RemoveRange(db.ClothingTypes);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoClothingTypes()
    {
        HttpResponseMessage response = await client.GetAsync("/api/clothing-types");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<ClothingTypeReadDTO>? result = await response.Content.ReadFromJsonAsync<List<ClothingTypeReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnClothingTypes_WhenExist()
    {
        Guid id = await SeedClothingTypeAsync("T-Shirt", "t-shirt");

        HttpResponseMessage response = await client.GetAsync("/api/clothing-types");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<ClothingTypeReadDTO>? result = await response.Content.ReadFromJsonAsync<List<ClothingTypeReadDTO>>();
        result!.Any(ct => ct.Id == id).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        Guid id = await SeedClothingTypeAsync("Hoodie", "hoodie");

        HttpResponseMessage response = await client.GetAsync($"/api/clothing-types/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ClothingTypeReadDTO result = await response.Content.ReadFromJsonAsync<ClothingTypeReadDTO>();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Hoodie");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/clothing-types/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        ClothingTypeCreateDTO dto = new ClothingTypeCreateDTO
        {
            Name = "Jacket",
            Slug = "jacket"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothing-types", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        ClothingTypeCreateDTO dto = new ClothingTypeCreateDTO
        {
            Name = "Jacket",
            Slug = "jacket"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothing-types", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        ClothingTypeCreateDTO dto = new ClothingTypeCreateDTO
        {
            Name = "Pants",
            Slug = "pants"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothing-types", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ClothingTypeReadDTO result = await response.Content.ReadFromJsonAsync<ClothingTypeReadDTO>();
        result!.Name.Should().Be("Pants");
        result.Slug.Should().Be("pants");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ClothingTypeCreateDTO dto = new ClothingTypeCreateDTO
        {
            Name = "Shorts",
            Slug = "shorts"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothing-types", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedClothingTypeAsync("Dress", "dress");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ClothingTypeCreateDTO dto = new ClothingTypeCreateDTO
        {
            Name = "Dress",
            Slug = "dress-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothing-types", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        await SeedClothingTypeAsync("Coat", "coat");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ClothingTypeCreateDTO dto = new ClothingTypeCreateDTO
        {
            Name = "Coat 2",
            Slug = "coat"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/clothing-types", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedClothingTypeAsync("OldType", "old-type");
        ClothingTypeUpdateDTO dto = new ClothingTypeUpdateDTO
        {
            Name = "NewType",
            Slug = "new-type"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothing-types/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedData()
    {
        Guid id = await SeedClothingTypeAsync("TypeToUpdate", "type-to-update");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        ClothingTypeUpdateDTO dto = new ClothingTypeUpdateDTO
        {
            Name = "UpdatedType",
            Slug = "updated-type"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothing-types/{id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ClothingTypeReadDTO result = await response.Content.ReadFromJsonAsync<ClothingTypeReadDTO>();
        result!.Name.Should().Be("UpdatedType");
        result.Slug.Should().Be("updated-type");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ClothingTypeUpdateDTO dto = new ClothingTypeUpdateDTO
        {
            Name = "Ghost",
            Slug = "ghost"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothing-types/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenNameTakenByAnother()
    {
        await SeedClothingTypeAsync("ExistingType", "existing-type");
        Guid id = await SeedClothingTypeAsync("TypeToEdit", "type-to-edit");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ClothingTypeUpdateDTO dto = new ClothingTypeUpdateDTO { Name = "ExistingType", Slug = "type-to-edit" };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/clothing-types/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedClothingTypeAsync("ToDelete", "to-delete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/clothing-types/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid id = await SeedClothingTypeAsync("ToDeleteManager", "to-delete-manager");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothing-types/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid id = await SeedClothingTypeAsync("ToDeleteAdmin", "to-delete-admin");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/clothing-types/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/clothing-types/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedClothingTypeAsync(string name, string slug)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        ClothingType entity = new ClothingType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        db.ClothingTypes.Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}