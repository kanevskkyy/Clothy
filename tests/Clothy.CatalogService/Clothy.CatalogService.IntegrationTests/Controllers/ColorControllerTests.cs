using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class ColorControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public ColorControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.Colors.RemoveRange(db.Colors);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoColors()
    {
        HttpResponseMessage response = await client.GetAsync("/api/colors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<ColorReadDTO>? result = await response.Content.ReadFromJsonAsync<List<ColorReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnColors_WhenExist()
    {
        Guid id = await SeedColorAsync("Red", "#FF0000", "red");

        HttpResponseMessage response = await client.GetAsync("/api/colors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<ColorReadDTO>? result = await response.Content.ReadFromJsonAsync<List<ColorReadDTO>>();
        result!.Any(c => c.Id == id).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        Guid id = await SeedColorAsync("Blue", "#0000FF", "blue");

        HttpResponseMessage response = await client.GetAsync($"/api/colors/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ColorReadDTO? result = await response.Content.ReadFromJsonAsync<ColorReadDTO>();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Blue");
        result.HexCode.Should().Be("#0000FF");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/colors/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "Green",
            HexCode = "#00FF00",
            Slug = "green"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "Green",
            HexCode = "#00FF00",
            Slug = "green"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "Yellow",
            HexCode = "#FFFF00",
            Slug = "yellow"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ColorReadDTO? result = await response.Content.ReadFromJsonAsync<ColorReadDTO>();
        result!.Name.Should().Be("Yellow");
        result.HexCode.Should().Be("#FFFF00");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "Purple",
            HexCode = "#800080",
            Slug = "purple"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenHexAlreadyExists()
    {
        await SeedColorAsync("Black", "#000000", "black");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "Black 2",
            HexCode = "#000000",
            Slug = "black-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedColorAsync("White", "#FFFFFF", "white");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "White",
            HexCode = "#FFFFFE",
            Slug = "white-2"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenSlugAlreadyExists()
    {
        await SeedColorAsync("Orange", "#FFA500", "orange");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ColorCreateDTO dto = new ColorCreateDTO
        {
            Name = "Orange 2",
            HexCode = "#FFA501",
            Slug = "orange"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/colors", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedColorAsync("OldColor", "#111111", "old-color");
        ColorUpdateDTO dto = new ColorUpdateDTO
        {
            Name = "NewColor",
            HexCode = "#222222",
            Slug = "new-color"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/colors/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedData()
    {
        Guid id = await SeedColorAsync("ColorToUpdate", "#333333", "color-to-update");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        ColorUpdateDTO dto = new ColorUpdateDTO
        {
            Name = "UpdatedColor",
            HexCode = "#444444",
            Slug = "updated-color"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/colors/{id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ColorReadDTO? result = await response.Content.ReadFromJsonAsync<ColorReadDTO>();
        result!.Name.Should().Be("UpdatedColor");
        result.HexCode.Should().Be("#444444");
    }

    [Fact]
    public async Task Update_ShouldReturnConflict_WhenHexTakenByAnother()
    {
        await SeedColorAsync("ExistingColor", "#555555", "existing-color");
        Guid id = await SeedColorAsync("ColorToEdit", "#666666", "color-to-edit");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        ColorUpdateDTO dto = new ColorUpdateDTO
        {
            Name = "ColorToEdit",
            HexCode = "#555555",
            Slug = "color-to-edit"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/colors/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedColorAsync("ToDelete", "#777777", "to-delete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/colors/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid id = await SeedColorAsync("ToDeleteManager", "#888888", "to-delete-manager");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/colors/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid id = await SeedColorAsync("ToDeleteAdmin", "#999999", "to-delete-admin");
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/colors/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/colors/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedColorAsync(string name, string hexCode, string slug)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();

        Color entity = new Color
        {
            Id = Guid.NewGuid(),
            Name = name,
            HexCode = hexCode,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        db.Colors.Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}