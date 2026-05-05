using Clothy.OrderService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clothy.OrderService.ContractTests.Provider;

[Collection("OrderContract")]
public class OrderStatsContractTests : IAsyncLifetime
{
    private OrderGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private OrderContractSeedHelper.SeedData seed = null!;
    private string connectionString = null!;

    public OrderStatsContractTests(OrderGrpcWebFactory factory)
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
    public async Task GetOrderStats_WhenCalled_ReturnsStats()
    {
        OrderStatsService.OrderStatsServiceClient client = new(channel);

        OrderStats response = await client.GetOrderStatsAsync(new Google.Protobuf.WellKnownTypes.Empty());

        Assert.True(response.NewOrdersCount >= 0);
        Assert.False(string.IsNullOrEmpty(response.TotalPrice));
        Assert.True(response.PendingOrders >= 0);
    }

    [Fact]
    public async Task GetOrderStats_ResponseContract_HasRequiredFields()
    {
        OrderStatsService.OrderStatsServiceClient client = new(channel);

        OrderStats response = await client.GetOrderStatsAsync(new Google.Protobuf.WellKnownTypes.Empty());

        Assert.IsType<int>(response.NewOrdersCount);
        Assert.IsType<string>(response.TotalPrice);
        Assert.IsType<int>(response.PendingOrders);
        Assert.True(decimal.TryParse(response.TotalPrice,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out _));
    }
}