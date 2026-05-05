using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class MaterialControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public MaterialControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.Materials.RemoveRange(db.Materials);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoMaterials()
    {
        HttpResponseMessage response = await client.GetAsync("/api/materials");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<MaterialReadDTO>? result = await response.Content.ReadFromJsonAsync<List<MaterialReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnMaterials_WhenExist()
    {
        Guid id = await SeedMaterialAsync("Cotton", "cotton");

        HttpResponseMessage response = await client.GetAsync("/api/materials");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<MaterialReadDTO>? result = await response.Content.ReadFromJsonAsync<List<MaterialReadDTO>>();
        result!.Any(m => m.Id == id).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        Guid id = await SeedMaterialAsync("Polyester", "polyester");

        HttpResponseMessage response = await client.GetAsync($"/api/materials/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        MaterialReadDTO? result = await response.Content.ReadFromJsonAsync<MaterialReadDTO>();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Polyester");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/materials/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        MaterialCreateDTO dto = new MaterialCreateDTO
        {
            Name = "Silk",
            Slug = "silk"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/materials", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        MaterialCreateDTO dto = new MaterialCreateDTO
        {
            Name = "Silk",
            Slug = "silk"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/materials", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        MaterialCreateDTO dto = new MaterialCreateDTO
        {
            Name = "Linen",
            Slug = "linen"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/materials", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<MaterialReadDTO>();
        result!.Name.Should().Be("Linen");
        result.Slug.Should().Be("linen");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MaterialCreateDTO dto = new MaterialCreateDTO
        {
            Name = "Wool",
            Slug = "wool"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/materials", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedMaterialAsync("Denim", "denim");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MaterialCreateDTO dto = new MaterialCreateDTO
        {
            Name = "Denim",
            Slug = "denim-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/materials", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        await SeedMaterialAsync("Velvet", "velvet");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MaterialCreateDTO dto = new MaterialCreateDTO
        {
            Name = "Velvet 2",
            Slug = "velvet"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/materials", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedMaterialAsync("OldMaterial", "old-material");
        MaterialUpdateDTO dto = new MaterialUpdateDTO
        {
            Name = "NewMaterial",
            Slug = "new-material"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/materials/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedData()
    {
        Guid id = await SeedMaterialAsync("MaterialToUpdate", "material-to-update");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        MaterialUpdateDTO dto = new MaterialUpdateDTO
        {
            Name = "UpdatedMaterial",
            Slug = "updated-material"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/materials/{id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        MaterialReadDTO? result = await response.Content.ReadFromJsonAsync<MaterialReadDTO>();
        result!.Name.Should().Be("UpdatedMaterial");
        result.Slug.Should().Be("updated-material");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MaterialUpdateDTO dto = new MaterialUpdateDTO
        {
            Name = "Ghost",
            Slug = "ghost"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/materials/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenNameTakenByAnother()
    {
        await SeedMaterialAsync("ExistingMaterial", "existing-material");
        Guid id = await SeedMaterialAsync("MaterialToEdit", "material-to-edit");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MaterialUpdateDTO dto = new MaterialUpdateDTO
        {
            Name = "ExistingMaterial",
            Slug = "material-to-edit"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/materials/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedMaterialAsync("ToDelete", "to-delete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/materials/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid id = await SeedMaterialAsync("ToDeleteManager", "to-delete-manager");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/materials/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid id = await SeedMaterialAsync("ToDeleteAdmin", "to-delete-admin");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/materials/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/materials/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedMaterialAsync(string name, string slug)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        Material entity = new Material
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };
        db.Materials.Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}