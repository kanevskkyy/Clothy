using Clothy.CatalogService.ContractTests.Infrastructure;
using Grpc.Net.Client;
using Xunit;

namespace Clothy.CatalogService.ContractTests.Provider;

[Collection("CatalogContract")]
public class ValidateOrderItemsContractTests : IAsyncLifetime
{
    private CatalogGrpcWebFactory factory;
    private GrpcChannel channel = null!;
    private CatalogContractSeedHelper.SeedData seedData = null!;

    public ValidateOrderItemsContractTests(CatalogGrpcWebFactory factory)
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
    public async Task ValidateOrderItems_WhenItemsAreValid_ReturnsValidResults()
    {
        OrderItemValidator.OrderItemValidatorClient client = new(channel);

        ValidateOrderItemsResponse response = await client.ValidateOrderItemsAsync(
            new ValidateOrderItemsRequest
            {
                Items =
                {
                    new OrderItemToValidate
                    {
                        ClotheId = seedData.ClotheId.ToString(),
                        ColorId = seedData.ColorId.ToString(),
                        SizeId = seedData.SizeId.ToString(),
                        Quantity = 1
                    }
                }
            });

        Assert.NotNull(response);
        Assert.Single(response.Results);
        Assert.True(response.Results[0].IsValid);
        Assert.Equal("Contract Test Jacket", response.Results[0].ClotheName);
        Assert.False(string.IsNullOrEmpty(response.Results[0].Price));
        Assert.Equal(seedData.HexCode, response.Results[0].ColorHexCode);
    }

    [Fact]
    public async Task ValidateOrderItems_WhenQuantityExceedsStock_ReturnsInvalidResult()
    {
        OrderItemValidator.OrderItemValidatorClient client = new(channel);

        ValidateOrderItemsResponse response = await client.ValidateOrderItemsAsync(
            new ValidateOrderItemsRequest
            {
                Items =
                {
                    new OrderItemToValidate
                    {
                        ClotheId = seedData.ClotheId.ToString(),
                        ColorId = seedData.ColorId.ToString(),
                        SizeId = seedData.SizeId.ToString(),
                        Quantity = 999
                    }
                }
            });

        Assert.NotNull(response);
        Assert.Single(response.Results);
        Assert.False(response.Results[0].IsValid);
        Assert.Contains("stock", response.Results[0].ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateOrderItems_WhenClotheIdIsInvalid_ReturnsInvalidResult()
    {
        OrderItemValidator.OrderItemValidatorClient client = new(channel);

        ValidateOrderItemsResponse response = await client.ValidateOrderItemsAsync(
            new ValidateOrderItemsRequest
            {
                Items =
                {
                    new OrderItemToValidate
                    {
                        ClotheId = Guid.NewGuid().ToString(),
                        ColorId = seedData.ColorId.ToString(),
                        SizeId = seedData.SizeId.ToString(),
                        Quantity = 1
                    }
                }
            });

        Assert.NotNull(response);
        Assert.Single(response.Results);
        Assert.False(response.Results[0].IsValid);
    }

    [Fact]
    public async Task ValidateOrderItems_WhenEmptyList_ThrowsRpcException()
    {
        OrderItemValidator.OrderItemValidatorClient client = new(channel);

        Grpc.Core.RpcException ex = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
            await client.ValidateOrderItemsAsync(new ValidateOrderItemsRequest()));

        Assert.Equal(Grpc.Core.StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task ValidateOrderItems_ResponseContract_HasRequiredFields()
    {
        OrderItemValidator.OrderItemValidatorClient client = new(channel);

        ValidateOrderItemsResponse response = await client.ValidateOrderItemsAsync(
            new ValidateOrderItemsRequest
            {
                Items =
                {
                    new OrderItemToValidate
                    {
                        ClotheId = seedData.ClotheId.ToString(),
                        ColorId = seedData.ColorId.ToString(),
                        SizeId = seedData.SizeId.ToString(),
                        Quantity = 1
                    }
                }
            });

        ValidateOrderItemResponse result = response.Results[0];
        Assert.False(string.IsNullOrEmpty(result.ClotheName));
        Assert.False(string.IsNullOrEmpty(result.Price));
        Assert.False(string.IsNullOrEmpty(result.ColorHexCode));
        Assert.False(string.IsNullOrEmpty(result.SizeName));
        Assert.False(string.IsNullOrEmpty(result.MainPhotoUrl));
    }
}