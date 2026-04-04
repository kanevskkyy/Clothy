using System.Net;
using System.Net.Http.Json;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.Domain.Entities;
using Clothy.OrderService.IntegrationTests.Infrastructure;
using Clothy.Shared.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using StackExchange.Redis;
using Xunit;

namespace Clothy.OrderService.IntegrationTests.Controllers;

public class OrderControllerTests : IClassFixture<OrderServiceWebApplicationFactory>, IAsyncLifetime
{
    private HttpClient client;
    private OrderServiceWebApplicationFactory factory;

    public OrderControllerTests(OrderServiceWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(factory.PostgresConnectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand cmd = new NpgsqlCommand(@"
            DELETE FROM delivery_detail;
            DELETE FROM orders_reservations;
            DELETE FROM order_item;
            DELETE FROM orders;
            DELETE FROM pickup_points;
            DELETE FROM settlements;
            DELETE FROM regions;
            DELETE FROM delivery_provider;", connection);
        await cmd.ExecuteNonQueryAsync();

        IConnectionMultiplexer redis = factory.Services.GetRequiredService<IConnectionMultiplexer>();
        IDatabase db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints().First());
        foreach (var key in server.Keys(pattern: "order*"))
        {
            await db.KeyDeleteAsync(key);
        }
    }
    
    [Fact]
    public async Task GetPaged_ShouldReturnUnauthorized_WhenNoToken()
    {
        HttpResponseMessage response = await client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnForbidden_WhenUserRole()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        HttpResponseMessage response = await client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaged_AsAdmin_ShouldReturnOkWithEmptyList()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPaged_AsAdmin_ShouldReturnOrdersWhenExist()
    {
        Guid userId = Guid.NewGuid();
        await SeedOrderAsync(userId);

        client.AddAuthorizationHeader(userId, new[] { "Admin" });
        HttpResponseMessage response = await client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPagedMyOrders_ShouldReturnUnauthorized_WhenNoToken()
    {
        HttpResponseMessage response = await client.GetAsync("/api/orders/my");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPagedMyOrders_AsUser_ShouldReturnOk()
    {
        Guid userId = Guid.NewGuid();
        client.AddAuthorizationHeader(userId, new[] { "User" });

        HttpResponseMessage response = await client.GetAsync("/api/orders/my");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPagedMyOrders_ShouldReturnOnlyCurrentUserOrders()
    {
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();

        await SeedOrderAsync(userId);
        await SeedOrderAsync(otherUserId);

        client.AddAuthorizationHeader(userId, new[] { "User" });
        HttpResponseMessage response = await client.GetAsync("/api/orders/my");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<OrderReadDTO> result = await response.Content.ReadFromJsonAsync<PagedList<OrderReadDTO>>();
        result!.Items.Should().OnlyContain(o => o.UserId == userId);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnUnauthorized_WhenNoToken()
    {
        HttpResponseMessage response = await client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_AsOwner_ShouldReturnOk()
    {
        Guid userId = Guid.NewGuid();
        Guid orderId = await SeedOrderAsync(userId);

        client.AddAuthorizationHeader(userId, new[] { "User" });
        HttpResponseMessage response = await client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        OrderDetailDTO? result = await response.Content.ReadFromJsonAsync<OrderDetailDTO>();
        result!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetById_AsOtherUser_ShouldReturnForbidden()
    {
        Guid ownerId = Guid.NewGuid();
        Guid orderId = await SeedOrderAsync(ownerId);

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });
        HttpResponseMessage response = await client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_AsAdmin_ShouldReturnOkForAnyOrder()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());

        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
    {
        OrderCreateDTO dto = new OrderCreateDTO
        {
            PickupPointId = Guid.NewGuid()
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/orders", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenBasketIsEmpty()
    {
        Guid userId = Guid.NewGuid();

        factory.BasketGrpcClientMock
            .Setup(x => x.GetUserBasketAsync(userId))
            .ReturnsAsync(new GetUserBasketResponse { Items =
            {
                
            }});

        client.AddAuthorizationHeaderWithEmailConfirmed(userId, new[] { "User" });

        OrderCreateDTO dto = new OrderCreateDTO
        {
            PickupPointId = Guid.NewGuid()
        };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/orders", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());
        OrderUpdateStatusDTO dto = new OrderUpdateStatusDTO
        {
            Status = OrderStatus.Processing
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/orders/{orderId}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateStatus_AsUser_ShouldReturnForbidden()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "User" });

        OrderUpdateStatusDTO dto = new OrderUpdateStatusDTO 
        {
            Status = OrderStatus.Processing
        };
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/orders/{orderId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateStatus_AsManager_ShouldReturnOk()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        OrderUpdateStatusDTO dto = new OrderUpdateStatusDTO 
        {
            Status = OrderStatus.Processing
        };
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/orders/{orderId}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderDetailDTO>();
        result!.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        OrderUpdateStatusDTO dto = new OrderUpdateStatusDTO
        {
            Status = OrderStatus.Processing
        };

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/orders/{Guid.NewGuid()}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnUnauthorized_WhenNoToken()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());
        HttpResponseMessage response = await client.DeleteAsync($"/api/orders/{orderId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsManager_ShouldReturnForbidden()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Manager" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/orders/{orderId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldReturnNoContent()
    {
        Guid orderId = await SeedOrderAsync(Guid.NewGuid());
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });

        HttpResponseMessage response = await client.DeleteAsync($"/api/orders/{orderId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        client.AddAuthorizationHeader(Guid.NewGuid(), new[] { "Admin" });
        HttpResponseMessage response = await client.DeleteAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SeedOrderAsync(Guid userId, OrderStatus status = OrderStatus.AwaitingPayment)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(factory.PostgresConnectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand cmd = new NpgsqlCommand(@"
            INSERT INTO orders (id, status, userid, userfirstname, userlastname, useremail)
            VALUES (uuid_generate_v4(), @status, @userId, 'Test', 'User', 'test@test.com')
            RETURNING id", connection);

        cmd.Parameters.AddWithValue("status", (short)status);
        cmd.Parameters.AddWithValue("userId", userId);

        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<(Guid pickupPointId, Guid providerId, Guid settlementId, Guid regionId)>
        SeedPickupPointWithDependenciesAsync()
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(factory.PostgresConnectionString);
        await connection.OpenAsync();

        await using NpgsqlCommand providerCmd = new NpgsqlCommand(@"
            INSERT INTO delivery_provider (id, name, iconurl)
            VALUES (uuid_generate_v4(), 'Test Provider', 'https://example.com/icon.png')
            RETURNING id", connection);
        Guid providerId = (Guid)(await providerCmd.ExecuteScalarAsync())!;

        await using NpgsqlCommand regionCmd = new NpgsqlCommand(@"
            INSERT INTO regions (id, name, ref)
            VALUES (uuid_generate_v4(), 'Test Region', 'test-ref-region')
            RETURNING id", connection);
        Guid regionId = (Guid)(await regionCmd.ExecuteScalarAsync())!;

        await using NpgsqlCommand settlementCmd = new NpgsqlCommand(@"
            INSERT INTO settlements (id, name, type, regionid, ref)
            VALUES (uuid_generate_v4(), 'Test City', 1, @regionId, 'test-ref-settlement')
            RETURNING id", connection);
        settlementCmd.Parameters.AddWithValue("regionId", regionId);
        Guid settlementId = (Guid)(await settlementCmd.ExecuteScalarAsync())!;

        await using NpgsqlCommand pickupCmd = new NpgsqlCommand(@"
            INSERT INTO pickup_points (id, address, ref, deliveryproviderid, settlementid, isactive)
            VALUES (uuid_generate_v4(), 'Test Address', 'test-ref-pickup', @providerId, @settlementId, true)
            RETURNING id", connection);
        pickupCmd.Parameters.AddWithValue("providerId", providerId);
        pickupCmd.Parameters.AddWithValue("settlementId", settlementId);
        Guid pickupPointId = (Guid)(await pickupCmd.ExecuteScalarAsync())!;

        return (pickupPointId, providerId, settlementId, regionId);
    }
}