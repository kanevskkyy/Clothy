using Clothy.OrderService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clothy.OrderService.ContractTests.Provider;

[Collection("OrderContract")]
public class OrderHistoryContractTests : IAsyncLifetime
{
    private OrderGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private OrderContractSeedHelper.SeedData seed = null!;
    private string connectionString = null!;

    public OrderHistoryContractTests(OrderGrpcWebFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        channel = factory.CreateGrpcChannel();
        connectionString = factory.Services
            .GetRequiredService<IConfiguration>()
            .GetConnectionString("ClothyOrder")!;
        seed = await OrderContractSeedHelper.SeedOrderAsync(connectionString);
    }

    public async Task DisposeAsync()
    {
        await OrderContractSeedHelper.CleanAsync(connectionString);
        channel.Dispose();
    }

    [Fact]
    public async Task HasUserAlreadyOrdered_WhenUserHasOrders_ReturnsTrue()
    {
        OrderHistoryService.OrderHistoryServiceClient client = new(channel);

        HasUserAlreadyOrderedResponse response = await client.HasUserAlreadyOrderedAsync(new HasUserAlreadyOrderedRequest
            {
                UserId = seed.UserId.ToString()
            });

        Assert.True(response.HasOrdered);
    }

    [Fact]
    public async Task HasUserAlreadyOrdered_WhenUserHasNoOrders_ReturnsFalse()
    {
        OrderHistoryService.OrderHistoryServiceClient client = new(channel);

        HasUserAlreadyOrderedResponse response = await client.HasUserAlreadyOrderedAsync(new HasUserAlreadyOrderedRequest
            {
                UserId = Guid.NewGuid().ToString()
            });

        Assert.False(response.HasOrdered);
    }

    [Fact]
    public async Task HasUserAlreadyOrdered_ResponseContract_HasOrderedIsBool()
    {
        OrderHistoryService.OrderHistoryServiceClient client = new(channel);

        HasUserAlreadyOrderedResponse response = await client.HasUserAlreadyOrderedAsync(new HasUserAlreadyOrderedRequest
        {
            UserId = seed.UserId.ToString()
        });

        Assert.IsType<bool>(response.HasOrdered);
    }
}