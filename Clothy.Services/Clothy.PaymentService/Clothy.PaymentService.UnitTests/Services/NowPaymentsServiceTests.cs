using Clothy.PaymentService.BLL.Services;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Helpers.Exceptions;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Security.Claims;
using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.DTOs;
using Xunit;
 
namespace Clothy.PaymentService.UnitTests.Services;
 
public class NowPaymentsServiceTests
{
    private Mock<IGetOrderInfoClient> orderInfoClientMock;
    private Mock<ILogger<NowPaymentsService>> loggerMock;
    private Mock<IUserClaimsExtractor> userClaimsExtractorMock;
    private Mock<IPublishEndpoint> publishEndpointMock;
    private Mock<IOptions<CryptoSettings>> cryptoSettingsMock;
    private Mock<IHttpClientFactory> httpClientFactoryMock;
    private PaymentDbContext dbContext;
 
    private NowPaymentsService nowPaymentsService;
 
    public NowPaymentsServiceTests()
    {
        orderInfoClientMock = new Mock<IGetOrderInfoClient>();
        loggerMock = new Mock<ILogger<NowPaymentsService>>();
        userClaimsExtractorMock = new Mock<IUserClaimsExtractor>();
        publishEndpointMock = new Mock<IPublishEndpoint>();
        cryptoSettingsMock = new Mock<IOptions<CryptoSettings>>();
        httpClientFactoryMock = new Mock<IHttpClientFactory>();
 
        cryptoSettingsMock.Setup(s => s.Value).Returns(new CryptoSettings
        {
            ApiKey = "test-api-key",
            ApiKeyHeader = "x-api-key",
            BaseURL = "https://api.nowpayments.io/v1/invoice",
            CallbackURL = "https://callback.url",
            SuccessURL = "https://success.url",
            CancelURL = "https://cancel.url",
            PriceCurrency = "usd",
            WebhookSecret = "test-webhook-secret"
        });
 
        httpClientFactoryMock
            .Setup(f => f.CreateClient("NowPayments"))
            .Returns(new HttpClient());
 
        DbContextOptions<PaymentDbContext> options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        dbContext = new PaymentDbContext(options);
 
        nowPaymentsService = new NowPaymentsService(
            dbContext,
            orderInfoClientMock.Object,
            loggerMock.Object,
            cryptoSettingsMock.Object,
            userClaimsExtractorMock.Object,
            httpClientFactoryMock.Object,
            publishEndpointMock.Object);
    }
 
    private ClaimsPrincipal BuildClaims(Guid userId)
    {
        ClaimsPrincipal claims = new ClaimsPrincipal();
        userClaimsExtractorMock.Setup(e => e.GetUserId(claims)).Returns(userId);
        return claims;
    }
 
    private GetOrderInfoResponse BuildOrderInfoResponse(Guid orderId, Guid userId, OrderStatusGrpc status = OrderStatusGrpc.AwaitingPayment)
    {
        return new GetOrderInfoResponse
        {
            OrderId = orderId.ToString(),
            UserId = userId.ToString(),
            Price = "100.00",
            Status = status
        };
    }
 
    private void SetupHttpClient(HttpStatusCode statusCode, string responseJson)
    {
        Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseJson)
            });
 
        HttpClient httpClient = new HttpClient(handlerMock.Object);
        httpClientFactoryMock.Setup(f => f.CreateClient("NowPayments")).Returns(httpClient);
    }
    
    [Fact]
    public async Task CreatePaymentAsync_WhenOrderAlreadyPaid_ThrowsValidationFailedException()
    {
        Guid orderId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(userId);
 
        orderInfoClientMock
            .Setup(c => c.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOrderInfoResponse(orderId, userId, OrderStatusGrpc.Processing));
 
        Func<Task> act = async () => await nowPaymentsService.CreatePaymentAsync(
            new CreatePaymentRequestDTO { OrderId = orderId }, claims, CancellationToken.None);
 
        await act.Should().ThrowAsync<ValidationFailedException>()
            .WithMessage($"*{orderId}*already been paid*");
    }
 
    [Fact]
    public async Task CreatePaymentAsync_WhenUserIsNotOrderOwner_ThrowsForbiddenException()
    {
        Guid orderId = Guid.NewGuid();
        Guid orderOwnerId = Guid.NewGuid();
        Guid requestingUserId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(requestingUserId);
 
        orderInfoClientMock
            .Setup(c => c.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOrderInfoResponse(orderId, orderOwnerId, OrderStatusGrpc.AwaitingPayment));
 
        Func<Task> act = async () => await nowPaymentsService.CreatePaymentAsync(
            new CreatePaymentRequestDTO { OrderId = orderId }, claims, CancellationToken.None);
 
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*someone else's order*");
    }
    
    [Fact]
    public async Task RetryPaymentAsync_WhenPaymentNotFound_ThrowsNotFoundException()
    {
        Guid paymentId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(Guid.NewGuid());
 
        Func<Task> act = async () => await nowPaymentsService.RetryPaymentAsync(
            paymentId, claims, CancellationToken.None);
 
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{paymentId}*");
    }
    
    [Fact]
    public async Task HandleWebhookAsync_WhenSignatureIsInvalid_ReturnsWithoutThrowing()
    {
        string payload = "{\"payment_status\": \"finished\", \"order_id\": \"some-id\"}";
        string invalidSignature = "invalidsignature";
 
        Func<Task> act = async () => await nowPaymentsService.HandleWebhookAsync(
            payload, invalidSignature, CancellationToken.None);
 
        await act.Should().NotThrowAsync();
    }
 
    [Fact]
    public async Task HandleWebhookAsync_WhenSignatureIsInvalid_DoesNotPublishEvent()
    {
        string payload = "{\"payment_status\": \"finished\", \"order_id\": \"some-id\"}";
        string invalidSignature = "invalidsignature";
 
        await nowPaymentsService.HandleWebhookAsync(payload, invalidSignature, CancellationToken.None);
 
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<OrderPaidEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
 
    [Fact]
    public async Task HandleWebhookAsync_WhenStatusIsNotFinished_DoesNotPublishEvent()
    {
        string payload = "{\"payment_status\": \"waiting\", \"order_id\": \"some-id\"}";
 
        using System.Security.Cryptography.HMACSHA512 hmac = new System.Security.Cryptography.HMACSHA512(
            System.Text.Encoding.UTF8.GetBytes("test-webhook-secret"));
        byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        string validSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
 
        await nowPaymentsService.HandleWebhookAsync(payload, validSignature, CancellationToken.None);
 
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<OrderPaidEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
 
    [Fact]
    public async Task HandleWebhookAsync_WhenSignatureIsEmpty_DoesNotPublishEvent()
    {
        string payload = "{\"payment_status\": \"finished\", \"order_id\": \"some-id\"}";
        string emptySignature = "";
 
        await nowPaymentsService.HandleWebhookAsync(payload, emptySignature, CancellationToken.None);
 
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<OrderPaidEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}