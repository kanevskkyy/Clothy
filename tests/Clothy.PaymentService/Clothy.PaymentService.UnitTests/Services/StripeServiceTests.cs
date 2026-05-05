using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.DTOs;
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
using System.Security.Claims;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Services;

public class StripeServiceTests
{
    private Mock<IGetOrderInfoClient> orderInfoClientMock;
    private Mock<ILogger<StripeService>> loggerMock;
    private Mock<IUserClaimsExtractor> userClaimsExtractorMock;
    private Mock<IPublishEndpoint> publishEndpointMock;
    private Mock<IOptions<CardSettings>> stripeSettingsMock;
    private PaymentDbContext dbContext;

    private StripeService stripeService;

    public StripeServiceTests()
    {
        orderInfoClientMock = new Mock<IGetOrderInfoClient>();
        loggerMock = new Mock<ILogger<StripeService>>();
        userClaimsExtractorMock = new Mock<IUserClaimsExtractor>();
        publishEndpointMock = new Mock<IPublishEndpoint>();
        stripeSettingsMock = new Mock<IOptions<CardSettings>>();

        stripeSettingsMock.Setup(s => s.Value).Returns(new CardSettings
        {
            ApiKey = "sk_test_fake",
            SuccessURL = "https://success.url",
            CancelURL = "https://cancel.url",
            WebhookSecret = "whsec_fake"
        });

        DbContextOptions<PaymentDbContext> options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        dbContext = new PaymentDbContext(options);

        stripeService = new StripeService(
            dbContext,
            orderInfoClientMock.Object,
            loggerMock.Object,
            stripeSettingsMock.Object,
            userClaimsExtractorMock.Object,
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
    
    [Fact]
    public async Task CreatePaymentAsync_WhenOrderAlreadyPaid_ThrowsValidationFailedException()
    {
        Guid orderId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(userId);

        orderInfoClientMock
            .Setup(c => c.GetOrderInfoAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildOrderInfoResponse(orderId, userId, OrderStatusGrpc.Processing));

        Func<Task> act = async () => await stripeService.CreatePaymentAsync(
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

        Func<Task> act = async () => await stripeService.CreatePaymentAsync(
            new CreatePaymentRequestDTO { OrderId = orderId }, claims, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*someone else's order*");
    }

    [Fact]
    public async Task RetryPaymentAsync_WhenPaymentNotFound_ThrowsNotFoundException()
    {
        Guid paymentId = Guid.NewGuid();
        ClaimsPrincipal claims = BuildClaims(Guid.NewGuid());

        Func<Task> act = async () => await stripeService.RetryPaymentAsync(
            paymentId, claims, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{paymentId}*");
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenSignatureIsInvalid_ReturnsWithoutThrowing()
    {
        string invalidPayload = "{}";
        string invalidSignature = "invalid_signature";

        Func<Task> act = async () => await stripeService.HandleWebhookAsync(
            invalidPayload, invalidSignature, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenSignatureIsInvalid_DoesNotPublishEvent()
    {
        string invalidPayload = "{}";
        string invalidSignature = "invalid_signature";

        await stripeService.HandleWebhookAsync(invalidPayload, invalidSignature, CancellationToken.None);

        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<OrderPaidEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}