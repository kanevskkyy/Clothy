using Clothy.BasketService.ContractTests.Infrastructure;
using Grpc.Core;
using Grpc.Net.Client;
using Moq;
using Xunit;

namespace Clothy.BasketService.ContractTests.Provider;

[Collection("BasketContract")]
public class BasketGrpcContractTests : IDisposable
{
    private BasketGrpcWebFactory factory;
    private GrpcChannel channel;
    private Guid testUserId = Guid.NewGuid();

    public BasketGrpcContractTests(BasketGrpcWebFactory factory)
    {
        this.factory = factory;
        channel = factory.CreateGrpcChannel();

        factory.OrderItemValidatorMock
            .Setup(x => x.ValidateOrderItemsAsync(It.IsAny<List<OrderItemToValidate>>()))
            .ReturnsAsync(new ValidateOrderItemsResponse
            {
                Results =
                {
                    new ValidateOrderItemResponse
                    {
                        IsValid = true,
                        ClotheName = "Contract Clothe",
                        Price = "100",
                        ColorHexCode = "#000000",
                        SizeName = "M",
                        MainPhotoUrl = "https://test.com/photo.jpg",
                        ColorName = "Black",
                        ColorSlug = "black",
                        ClotheSlug = "contract-clothe"
                    }
                }
            });
    }

    [Fact]
    public async Task GetUserBasket_WhenBasketIsEmpty_ReturnsEmptyBasket()
    {
        BasketGrpc.BasketGrpcClient client = new(channel);

        GetUserBasketResponse response = await client.GetUserBasketAsync(new GetUserBasketRequest
        {
            UserId = testUserId.ToString()
        });

        Assert.Equal(testUserId.ToString(), response.UserId);
        Assert.Empty(response.Items);
    }

    [Fact]
    public async Task GetUserBasket_ResponseContract_HasRequiredFields()
    {
        BasketGrpc.BasketGrpcClient client = new(channel);

        GetUserBasketResponse response = await client.GetUserBasketAsync(new GetUserBasketRequest
        {
            UserId = testUserId.ToString()
        });

        Assert.False(string.IsNullOrEmpty(response.UserId));
        Assert.NotNull(response.Items);
    }

    [Fact]
    public async Task GetUserBasket_WhenInvalidGuid_ThrowsInvalidArgument()
    {
        BasketGrpc.BasketGrpcClient client = new(channel);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetUserBasketAsync(new GetUserBasketRequest
            {
                UserId = "not-a-guid"
            }));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task ClearUserBasket_WhenCalled_ReturnsSuccess()
    {
        BasketGrpc.BasketGrpcClient client = new(channel);

        ClearUserBasketResponse response = await client.ClearUserBasketAsync(new ClearUserBasketRequest
        {
            UserId = testUserId.ToString()
        });

        Assert.True(response.Success);
    }

    [Fact]
    public async Task ClearUserBasket_ResponseContract_SuccessIsBool()
    {
        BasketGrpc.BasketGrpcClient client = new(channel);

        ClearUserBasketResponse response = await client.ClearUserBasketAsync(new ClearUserBasketRequest
        {
            UserId = testUserId.ToString()
        });

        Assert.IsType<bool>(response.Success);
    }

    [Fact]
    public async Task ClearUserBasket_WhenInvalidGuid_ThrowsInvalidArgument()
    {
        BasketGrpc.BasketGrpcClient client = new(channel);

        RpcException ex = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.ClearUserBasketAsync(new ClearUserBasketRequest
            {
                UserId = "not-a-guid"
            }));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    public void Dispose() => channel.Dispose();
}