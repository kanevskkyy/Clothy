using Clothy.OrderService.ContractTests.Infrastructure;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clothy.OrderService.ContractTests.Provider;

[Collection("OrderContract")]
public class GetOrderInfoContractTests : IAsyncLifetime
{
    private OrderGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private OrderContractSeedHelper.SeedData seed = null!;
    private string connectionString = null!;

    public GetOrderInfoContractTests(OrderGrpcWebFactory factory)
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
    public async Task GetOrderInfo_WhenOrderExists_ReturnsOrderInfo()
    {
        OrderServiceGrpc.OrderServiceGrpcClient client = new(channel);

        GetOrderInfoResponse response = await client.GetOrderInfoAsync(new GetOrderInfoRequest
        {
            OrderId = seed.OrderId.ToString()
        });

        Assert.Equal(seed.OrderId.ToString(), response.OrderId);
        Assert.Equal(seed.UserId.ToString(), response.UserId);
        Assert.False(string.IsNullOrEmpty(response.Price));
        Assert.Equal(OrderStatusGrpc.AwaitingPayment, response.Status);
    }

    [Fact]
    public async Task GetOrderInfo_ResponseContract_HasRequiredFields()
    {
        OrderServiceGrpc.OrderServiceGrpcClient client = new(channel);

        GetOrderInfoResponse response = await client.GetOrderInfoAsync(new GetOrderInfoRequest
            {
                OrderId = seed.OrderId.ToString()
            });

        Assert.False(string.IsNullOrEmpty(response.OrderId));
        Assert.False(string.IsNullOrEmpty(response.UserId));
        Assert.False(string.IsNullOrEmpty(response.Price));
        Assert.True(decimal.TryParse(response.Price,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out _));
        Assert.True(Enum.IsDefined(typeof(OrderStatusGrpc), response.Status));
    }

    [Fact]
    public async Task GetOrderInfo_WhenOrderNotFound_ThrowsInternal()
    {
        OrderServiceGrpc.OrderServiceGrpcClient client = new(channel);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetOrderInfoAsync(new GetOrderInfoRequest
            {
                OrderId = Guid.NewGuid().ToString()
            }));

        Assert.Equal(StatusCode.Internal, ex.StatusCode);
    }
}