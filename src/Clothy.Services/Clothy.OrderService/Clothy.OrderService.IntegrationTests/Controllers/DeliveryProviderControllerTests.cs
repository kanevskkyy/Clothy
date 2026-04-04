using System.Net;
using System.Net.Http.Json;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using StackExchange.Redis;
using Xunit;

namespace Clothy.OrderService.IntegrationTests.Controllers;

public class DeliveryProviderControllerTests : IClassFixture<OrderServiceWebApplicationFactory>
{
    private HttpClient client;
    private OrderServiceWebApplicationFactory factory;

    public DeliveryProviderControllerTests(OrderServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProviderExists()
    {
        Guid providerId = await SeedDeliveryProviderAsync("DHL", "https://example.com/dhl.png");

        HttpResponseMessage response = await client.GetAsync($"/api/delivery-providers/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        DeliveryProviderReadDTO? result = await response.Content.ReadFromJsonAsync<DeliveryProviderReadDTO>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(providerId);
        result.Name.Should().Be("DHL");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/delivery-providers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        MultipartFormDataContent content = BuildMultipartForm("UPS", "fake-image-content");

        HttpResponseMessage response = await client.PostAsync("/api/delivery-providers", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturnForbidden_WhenUserRole()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        MultipartFormDataContent content = BuildMultipartForm("FedEx", "fake-image-content");

        HttpResponseMessage response = await client.PostAsync("/api/delivery-providers", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_AsManager_ShouldReturnCreated()
    {
        factory.ImageServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync("https://example.com/uploaded.png");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        MultipartFormDataContent content = BuildMultipartForm("Meest Express", "fake-image-content");

        HttpResponseMessage response = await client.PostAsync("/api/delivery-providers", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        DeliveryProviderReadDTO? result = await response.Content.ReadFromJsonAsync<DeliveryProviderReadDTO>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Meest Express");
        result.IconUrl.Should().Be("https://example.com/uploaded.png");
    }

    [Fact]
    public async Task Create_AsAdmin_ShouldReturnCreated()
    {
        factory.ImageServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync("https://example.com/uploaded-admin.png");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent content = BuildMultipartForm("Ukrposhta", "fake-image-content");

        HttpResponseMessage response = await client.PostAsync("/api/delivery-providers", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        factory.ImageServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync("https://example.com/icon.png");

        await SeedDeliveryProviderAsync("Justin", "https://example.com/justin.png");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent content = BuildMultipartForm("Justin", "fake-image-content");

        HttpResponseMessage response = await client.PostAsync("/api/delivery-providers", content);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task Update_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid providerId = await SeedDeliveryProviderAsync("OldName", "https://example.com/old.png");
        MultipartFormDataContent content = BuildMultipartForm("NewName", "fake-image-content");

        HttpResponseMessage response = await client.PutAsync($"/api/delivery-providers/{providerId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsManager_ShouldReturnOkAndUpdatedName()
    {
        factory.ImageServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync("https://example.com/new-icon.png");

        Guid providerId = await SeedDeliveryProviderAsync("OldProvider", "https://example.com/old.png");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });
        MultipartFormDataContent content = BuildMultipartForm("UpdatedProvider", "fake-image-content");

        HttpResponseMessage response = await client.PutAsync($"/api/delivery-providers/{providerId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        DeliveryProviderReadDTO? result = await response.Content.ReadFromJsonAsync<DeliveryProviderReadDTO>();
        result!.Name.Should().Be("UpdatedProvider");
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        MultipartFormDataContent content = BuildMultipartForm("Ghost", "fake-image-content");

        HttpResponseMessage response = await client.PutAsync($"/api/delivery-providers/{Guid.NewGuid()}", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid providerId = await SeedDeliveryProviderAsync("ToDelete", "https://example.com/icon.png");

        HttpResponseMessage response = await client.DeleteAsync($"/api/delivery-providers/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid providerId = await SeedDeliveryProviderAsync("ToDeleteManager", "https://example.com/icon.png");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/delivery-providers/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        factory.ImageServiceMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        Guid providerId = await SeedDeliveryProviderAsync("ToDeleteAdmin", "https://example.com/icon.png");

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/delivery-providers/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/delivery-providers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    private async Task<Guid> SeedDeliveryProviderAsync(string name, string iconUrl)
    {
        await using Npgsql.NpgsqlConnection connection = new Npgsql.NpgsqlConnection(
            factory.PostgresConnectionString);
        await connection.OpenAsync();

        await using Npgsql.NpgsqlCommand cmd = new Npgsql.NpgsqlCommand(@"
            INSERT INTO delivery_provider (id, name, iconurl)
            VALUES (uuid_generate_v4(), @name, @iconUrl)
            RETURNING id", connection);

        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("iconUrl", iconUrl);

        object? result = await cmd.ExecuteScalarAsync();
        return (Guid)result!;
    }

    private static MultipartFormDataContent BuildMultipartForm(string name, string fileContent)
    {
        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "Name");

        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
        ByteArrayContent fileStreamContent = new ByteArrayContent(fileBytes);
        fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileStreamContent, "Icon", "test-icon.png");

        return content;
    }
}