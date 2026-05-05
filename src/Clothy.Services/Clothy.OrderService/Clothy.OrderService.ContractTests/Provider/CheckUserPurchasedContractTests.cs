using Clothy.OrderService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Clothy.OrderService.ContractTests.Provider;

[Collection("OrderContract")]
public class CheckUserPurchasedContractTests : IAsyncLifetime
{
    private OrderGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private OrderContractSeedHelper.SeedData seed = null!;
    private string connectionString = null!;

    public CheckUserPurchasedContractTests(OrderGrpcWebFactory factory)
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
    public async Task CheckUserPurchased_WhenUserPurchasedClothe_ReturnsTrue()
    {
        CheckUserPurchasedGrpc.CheckUserPurchasedGrpcClient client = new(channel);

        CheckUserPurchasedResponse response = await client.CheckUserPurchasedAsync(
            new CheckUserPurchasedRequest
            {
                UserId = seed.UserId.ToString(),
                ClotheId = seed.ClotheId.ToString()
            });

        Assert.True(response.Purchased);
        Assert.Equal("Contract Clothe", response.ClotheName);
        Assert.False(string.IsNullOrEmpty(response.ClothePhotoURL));
    }

    [Fact]
    public async Task CheckUserPurchased_WhenUserDidNotPurchase_ReturnsFalse()
    {
        CheckUserPurchasedGrpc.CheckUserPurchasedGrpcClient client = new(channel);

        CheckUserPurchasedResponse response = await client.CheckUserPurchasedAsync(
            new CheckUserPurchasedRequest
            {
                UserId = Guid.NewGuid().ToString(),
                ClotheId = Guid.NewGuid().ToString()
            });

        Assert.False(response.Purchased);
    }

    [Fact]
    public async Task CheckUserPurchased_ResponseContract_HasRequiredFields()
    {
        CheckUserPurchasedGrpc.CheckUserPurchasedGrpcClient client = new(channel);

        CheckUserPurchasedResponse response = await client.CheckUserPurchasedAsync(
            new CheckUserPurchasedRequest
            {
                UserId = seed.UserId.ToString(),
                ClotheId = seed.ClotheId.ToString()
            });

        Assert.IsType<bool>(response.Purchased);
        Assert.NotNull(response.ClotheName);
        Assert.NotNull(response.ClothePhotoURL);
    }
}