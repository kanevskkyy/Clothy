using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Moq;

namespace Clothy.PaymentService.IntegrationTests.Infrastructure;

public static class GrpcMockBuilder
{
    public static void SetupOrderAwaitingPayment(this Mock<IGetOrderInfoClient> mock, Guid orderId, Guid userId, decimal price = 199.99m)
    {
        mock.Setup(x => x.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrderInfoResponse
            {
                OrderId = orderId.ToString(),
                UserId = userId.ToString(),
                Price = price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                Status = OrderStatusGrpc.AwaitingPayment
            });
    }
    
    public static void SetupOrderAlreadyPaid(this Mock<IGetOrderInfoClient> mock, Guid orderId, Guid userId)
    {
        mock.Setup(x => x.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrderInfoResponse
            {
                OrderId = orderId.ToString(),
                UserId = userId.ToString(),
                Price = "199.99",
                Status = OrderStatusGrpc.Processing
            });
    }
    
    public static void SetupOrderServiceUnavailable(this Mock<IGetOrderInfoClient> mock, Guid orderId)
    {
        mock.Setup(x => x.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Grpc.Core.RpcException(
                new Grpc.Core.Status(Grpc.Core.StatusCode.Unavailable, "Service unavailable")));
    }
}