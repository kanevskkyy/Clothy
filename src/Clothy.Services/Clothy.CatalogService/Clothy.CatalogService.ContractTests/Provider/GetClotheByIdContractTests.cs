using Clothy.CatalogService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Xunit;

namespace Clothy.CatalogService.ContractTests.Provider;

[Collection("CatalogContract")]
public class GetClotheBySlugContractTests : IAsyncLifetime
{
    private readonly CatalogGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private CatalogContractSeedHelper.SeedData seedData = null!;

    public GetClotheBySlugContractTests(CatalogGrpcWebFactory factory)
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
    public async Task GetClotheBySlug_WhenSlugDoesNotExist_ThrowsRpcException()
    {
        ClotheServiceGrpc.ClotheServiceGrpcClient client = new(channel);

        Grpc.Core.RpcException ex = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
            await client.GetClotheBySlugAsync(
                new ClotheGrpcRequest
                {
                    Slug = "non-existent-slug"
                }));

        Assert.Equal(Grpc.Core.StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task GetClotheBySlug_WhenSlugIsEmpty_ThrowsRpcException()
    {
        ClotheServiceGrpc.ClotheServiceGrpcClient client = new(channel);

        Grpc.Core.RpcException ex = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
            await client.GetClotheBySlugAsync(
                new ClotheGrpcRequest
                {
                    Slug = ""
                }));

        Assert.Equal(Grpc.Core.StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task GetClotheBySlug_ResponseContract_HasRequiredFields()
    {
        ClotheServiceGrpc.ClotheServiceGrpcClient client = new(channel);

        ClotheDetailGrpcResponse response = await client.GetClotheBySlugAsync(
            new ClotheGrpcRequest
            {
                Slug = seedData.Slug
            });

        Assert.False(string.IsNullOrEmpty(response.Id));
        Assert.False(string.IsNullOrEmpty(response.Name));
        Assert.False(string.IsNullOrEmpty(response.Slug));
        Assert.False(string.IsNullOrEmpty(response.Price));
        Assert.False(string.IsNullOrEmpty(response.Gender));
        Assert.False(string.IsNullOrEmpty(response.Description));
        Assert.NotNull(response.Brand);
        Assert.False(string.IsNullOrEmpty(response.Brand.Id));
        Assert.False(string.IsNullOrEmpty(response.Brand.Name));
        Assert.NotNull(response.Collection);
        Assert.False(string.IsNullOrEmpty(response.Collection.Id));
    }
}