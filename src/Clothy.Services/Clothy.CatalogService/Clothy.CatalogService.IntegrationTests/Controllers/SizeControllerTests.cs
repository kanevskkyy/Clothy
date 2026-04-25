using System.Net;
using System.Net.Http.Json;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.Domain.Entities.Catalog;
using Clothy.CatalogService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Controllers;

[Collection("CatalogService")]
public class SizeControllerTests : IAsyncLifetime
{
    private HttpClient client;
    private CatalogServiceWebApplicationFactory factory;

    public SizeControllerTests(CatalogServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        db.Sizes.RemoveRange(db.Sizes);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoSizes()
    {
        HttpResponseMessage response = await client.GetAsync("/api/sizes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<SizeReadDTO>? result = await response.Content.ReadFromJsonAsync<List<SizeReadDTO>>();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnSizes_WhenExist()
    {
        Guid id = await SeedSizeAsync("XS");

        HttpResponseMessage response = await client.GetAsync("/api/sizes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<SizeReadDTO>? result = await response.Content.ReadFromJsonAsync<List<SizeReadDTO>>();
        result!.Any(s => s.Id == id).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        Guid id = await SeedSizeAsync("S");

        HttpResponseMessage response = await client.GetAsync($"/api/sizes/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        SizeReadDTO? result = await response.Content.ReadFromJsonAsync<SizeReadDTO>();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("S");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/sizes/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        SizeCreateDTO dto = new SizeCreateDTO
        {
            Name = "M"
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sizes", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsUser_ShouldReturnForbidden()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        SizeCreateDTO dto = new SizeCreateDTO
        {
            Name = "M"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sizes", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        SizeCreateDTO dto = new SizeCreateDTO
        {
            Name = "L"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sizes", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        SizeReadDTO? result = await response.Content.ReadFromJsonAsync<SizeReadDTO>();
        result!.Name.Should().Be("L");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        SizeCreateDTO dto = new SizeCreateDTO
        {
            Name = "XL"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sizes", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        await SeedSizeAsync("XXL");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        SizeCreateDTO dto = new SizeCreateDTO
        {
            Name = "XXL"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sizes", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedSizeAsync("OldSize");
        SizeUpdateDTO dto = new SizeUpdateDTO { Name = "NewSize" };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/sizes/{id}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        SizeUpdateDTO dto = new SizeUpdateDTO
        {
            Name = "Ghost"
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/sizes/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid id = await SeedSizeAsync("ToDelete");
        HttpResponseMessage response = await client.DeleteAsync($"/api/sizes/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/sizes/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedSizeAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        ClothyCatalogDbContext db = scope.ServiceProvider.GetRequiredService<ClothyCatalogDbContext>();
        Size entity = new Size
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
        db.Sizes.Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }
}