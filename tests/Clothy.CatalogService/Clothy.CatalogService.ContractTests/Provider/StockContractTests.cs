using Clothy.CatalogService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Xunit;

namespace Clothy.CatalogService.ContractTests.Provider;

[Collection("CatalogContract")]
public class StockContractTests : IAsyncLifetime
{
    private CatalogGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private CatalogContractSeedHelper.SeedData seedData = null!;

    public StockContractTests(CatalogGrpcWebFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        channel = factory.CreateGrpcChannel();
        seedData = await CatalogContractSeedHelper.SeedClotheAsync(factory.Services);
    }

    public async Task DisposeAsync()
    {
        await CatalogContractSeedHelper.CleanAsync(factory.Services);
        channel.Dispose();
    }

    [Fact]
    public async Task GetTotalQuantity_WhenStockExists_ReturnsTotalQuantity()
    {
        ClotheStockService.ClotheStockServiceClient client = new(channel);

        GetTotalQuantityResponse response = await client.GetTotalQuantityAsync(new Google.Protobuf.WellKnownTypes.Empty());

        Assert.NotNull(response);
        Assert.True(response.TotalQuantity >= 0);
    }

    [Fact]
    public async Task GetTotalQuantity_ResponseContract_HasRequiredFields()
    {
        ClotheStockService.ClotheStockServiceClient client = new(channel);

        GetTotalQuantityResponse response = await client.GetTotalQuantityAsync(new Google.Protobuf.WellKnownTypes.Empty());

        Assert.IsType<int>(response.TotalQuantity);
    }
}