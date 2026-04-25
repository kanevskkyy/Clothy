using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class CollectionControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public CollectionControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.Collections.RemoveRange(db.Collections);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoCollections()
    {
        HttpResponseMessage response = await client.GetAsync("/api/collections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<CollectionReadDTO>? result = await response.Content.ReadFromJsonAsync<List<CollectionReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnCollections_WhenExist()
    {
        Guid id = await SeedCollectionAsync("Summer 2024", "summer-2024");

        HttpResponseMessage response = await client.GetAsync("/api/collections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<CollectionReadDTO>? result = await response.Content.ReadFromJsonAsync<List<CollectionReadDTO>>();
        result!.Any(c => c.Id == id).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        Guid id = await SeedCollectionAsync("Winter 2024", "winter-2024");

        HttpResponseMessage response = await client.GetAsync($"/api/collections/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CollectionReadDTO? result = await response.Content.ReadFromJsonAsync<CollectionReadDTO>();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Winter 2024");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/collections/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        CollectionCreateDTO dto = new CollectionCreateDTO
        {
            Name = "Spring",
            Slug = "spring"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/collections", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        CollectionCreateDTO dto = new CollectionCreateDTO
        {
            Name = "Spring",
            Slug = "spring"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/collections", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        CollectionCreateDTO dto = new CollectionCreateDTO
        {
            Name = "Autumn 2024",
            Slug = "autumn-2024"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/collections", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CollectionReadDTO? result = await response.Content.ReadFromJsonAsync<CollectionReadDTO>();
        result!.Name.Should().Be("Autumn 2024");
        result.Slug.Should().Be("autumn-2024");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        CollectionCreateDTO dto = new CollectionCreateDTO
        {
            Name = "Holiday 2024",
            Slug = "holiday-2024"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/collections", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedCollectionAsync("Basics", "basics");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        CollectionCreateDTO dto = new CollectionCreateDTO
        {
            Name = "Basics",
            Slug = "basics-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/collections", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        await SeedCollectionAsync("Sport", "sport");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        CollectionCreateDTO dto = new CollectionCreateDTO
        {
            Name = "Sport 2",
            Slug = "sport"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/collections", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedCollectionAsync("OldCollection", "old-collection");
        CollectionUpdateDTO dto = new CollectionUpdateDTO
        {
            Name = "NewCollection",
            Slug = "new-collection"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/collections/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedData()
    {
        Guid id = await SeedCollectionAsync("CollectionToUpdate", "collection-to-update");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        CollectionUpdateDTO dto = new CollectionUpdateDTO
        {
            Name = "UpdatedCollection",
            Slug = "updated-collection"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/collections/{id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CollectionReadDTO? result = await response.Content.ReadFromJsonAsync<CollectionReadDTO>();
        result!.Name.Should().Be("UpdatedCollection");
        result.Slug.Should().Be("updated-collection");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        CollectionUpdateDTO dto = new CollectionUpdateDTO
        {
            Name = "Ghost",
            Slug = "ghost"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/collections/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenNameTakenByAnother()
    {
        await SeedCollectionAsync("ExistingCollection", "existing-collection");
        Guid id = await SeedCollectionAsync("CollectionToEdit", "collection-to-edit");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        CollectionUpdateDTO dto = new CollectionUpdateDTO
        {
            Name = "ExistingCollection",
            Slug = "collection-to-edit"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/collections/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedCollectionAsync("ToDelete", "to-delete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid id = await SeedCollectionAsync("ToDeleteManager", "to-delete-manager");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid id = await SeedCollectionAsync("ToDeleteAdmin", "to-delete-admin");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/collections/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedCollectionAsync(string name, string slug)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        Collection entity = new Collection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        db.Collections.Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}