using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Moq;
using Xunit;

namespace Clothy.PaymentService.ContractTests.Consumer;

public class PaymentOrderConsumerContractTests
{
    private Mock<IGetOrderInfoClient> mockClient = new();

    [Fact]
    public async Task GetOrderInfo_ConsumerExpects_OrderIdUserIdPriceStatus()
    {
        Guid orderId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        mockClient
            .Setup(x => x.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrderInfoResponse
            {
                OrderId = orderId.ToString(),
                UserId = userId.ToString(),
                Price = "250.00",
                Status = OrderStatusGrpc.AwaitingPayment
            });

        GetOrderInfoResponse response = await mockClient.Object.GetOrderInfoAsync(orderId);

        Assert.Equal(orderId.ToString(), response.OrderId);
        Assert.Equal(userId.ToString(), response.UserId);
        Assert.False(string.IsNullOrEmpty(response.Price));
        Assert.True(decimal.TryParse(response.Price,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out _));
    }

    [Fact]
    public async Task GetOrderInfo_ConsumerContract_ResponseHasAllRequiredFields()
    {
        Guid orderId = Guid.NewGuid();

        mockClient
            .Setup(x => x.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrderInfoResponse
            {
                OrderId = orderId.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Price = "100.00",
                Status = OrderStatusGrpc.AwaitingPayment
            });

        GetOrderInfoResponse response = await mockClient.Object.GetOrderInfoAsync(orderId);

        Assert.False(string.IsNullOrEmpty(response.OrderId));
        Assert.False(string.IsNullOrEmpty(response.UserId));
        Assert.False(string.IsNullOrEmpty(response.Price));
        Assert.True(Enum.IsDefined(typeof(OrderStatusGrpc), response.Status));
    }

    [Fact]
    public void GetOrderInfo_ConsumerContract_AllStatusEnumValuesAreKnown()
    {
        Assert.True(Enum.IsDefined(typeof(OrderStatusGrpc), OrderStatusGrpc.AwaitingPayment));
        Assert.True(Enum.IsDefined(typeof(OrderStatusGrpc), OrderStatusGrpc.Processing));
        Assert.True(Enum.IsDefined(typeof(OrderStatusGrpc), OrderStatusGrpc.Shipped));
        Assert.True(Enum.IsDefined(typeof(OrderStatusGrpc), OrderStatusGrpc.Delivered));
    }

    [Fact]
    public async Task GetOrderInfo_WhenPriceIsReturned_CanBeParsedAsDecimal()
    {
        Guid orderId = Guid.NewGuid();

        mockClient
            .Setup(x => x.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrderInfoResponse
            {
                OrderId = orderId.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Price = "1500.50",
                Status = OrderStatusGrpc.Processing
            });

        GetOrderInfoResponse response = await mockClient.Object.GetOrderInfoAsync(orderId);

        bool parsed = decimal.TryParse(response.Price,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal price);

        Assert.True(parsed);
        Assert.True(price > 0);
    }
}