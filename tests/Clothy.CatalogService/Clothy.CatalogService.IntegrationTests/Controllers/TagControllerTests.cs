using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class TagControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public TagControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.Tags.RemoveRange(db.Tags);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoTags()
    {
        HttpResponseMessage response = await client.GetAsync("/api/tags");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<TagReadDTO>? result = await response.Content.ReadFromJsonAsync<List<TagReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnTags_WhenExist()
    {
        Guid id = await SeedTagAsync("Sale", "sale");

        HttpResponseMessage response = await client.GetAsync("/api/tags");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<TagReadDTO>? result = await response.Content.ReadFromJsonAsync<List<TagReadDTO>>();
        result!.Any(t => t.Id == id).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        Guid id = await SeedTagAsync("New", "new");

        HttpResponseMessage response = await client.GetAsync($"/api/tags/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TagReadDTO? result = await response.Content.ReadFromJsonAsync<TagReadDTO>();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("New");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/tags/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        TagCreateDTO dto = new TagCreateDTO
        {
            Name = "Hot",
            Slug = "hot"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/tags", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        TagCreateDTO dto = new TagCreateDTO
        {
            Name = "Hot",
            Slug = "hot"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/tags", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        TagCreateDTO dto = new TagCreateDTO
        {
            Name = "Trending", 
            Slug = "trending" 
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/tags", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        TagReadDTO? result = await response.Content.ReadFromJsonAsync<TagReadDTO>();
        result!.Name.Should().Be("Trending");
        result.Slug.Should().Be("trending");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        TagCreateDTO dto = new TagCreateDTO 
        {
            Name = "Limited", 
            Slug = "limited"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/tags", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedTagAsync("Eco", "eco");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        TagCreateDTO dto = new TagCreateDTO
        {
            Name = "Eco",
            Slug = "eco-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/tags", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        await SeedTagAsync("Premium", "premium");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        TagCreateDTO dto = new TagCreateDTO
        {
            Name = "Premium 2",
            Slug = "premium"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/tags", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedTagAsync("OldTag", "old-tag");
        TagUpdateDTO dto = new TagUpdateDTO
        {
            Name = "NewTag", 
            Slug = "new-tag" 
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/tags/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedData()
    {
        Guid id = await SeedTagAsync("TagToUpdate", "tag-to-update");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        TagUpdateDTO dto = new TagUpdateDTO
        {
            Name = "UpdatedTag",
            Slug = "updated-tag"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/tags/{id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TagReadDTO? result = await response.Content.ReadFromJsonAsync<TagReadDTO>();
        result!.Name.Should().Be("UpdatedTag");
        result.Slug.Should().Be("updated-tag");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        TagUpdateDTO dto = new TagUpdateDTO
        {
            Name = "Ghost",
            Slug = "ghost"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/tags/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenNameTakenByAnother()
    {
        await SeedTagAsync("ExistingTag", "existing-tag");
        Guid id = await SeedTagAsync("TagToEdit", "tag-to-edit");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        TagUpdateDTO dto = new TagUpdateDTO
        {
            Name = "ExistingTag",
            Slug = "tag-to-edit"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/tags/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedTagAsync("ToDelete", "to-delete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/tags/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid id = await SeedTagAsync("ToDeleteManager", "to-delete-manager");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/tags/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid id = await SeedTagAsync("ToDeleteAdmin", "to-delete-admin");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/tags/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/tags/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedTagAsync(string name, string slug)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        
        Tag entity = new Tag { Id = Guid.NewGuid(), Name = name, Slug = slug, CreatedAt = DateTime.UtcNow };
        db.Tags.Add(entity);
        
        await db.SaveChangesAsync();
        return entity.Id;
    }
}