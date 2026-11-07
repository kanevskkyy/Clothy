using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.BLL.Services;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Clothy.PaymentService.UnitTests.Infrastructure;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Helpers.JWT;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;
using Stripe.Checkout;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Services;

/// <summary>Webhook routing with real signature checks (payloads signed with the test secret).</summary>
public class StripeWebhookFraudTests
{
    private const string WEBHOOK_SECRET = "whsec_test_secret";

    private readonly PaymentDbContext dbContext;
    private readonly Mock<IPublishEndpoint> publishMock = new();
    private readonly Mock<IStripeFraudScorer> scorerMock = new();
    private readonly Mock<IStripeDisputeHandler> disputeMock = new();
    private readonly StripeService service;
    private readonly PaymentRecordEF payment;

    public StripeWebhookFraudTests()
    {
        dbContext = new PaymentDbContext(new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        payment = new PaymentRecordEF
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Price = 50m,
            Status = PaymentStatus.Pending,
            TransactionId = "cs_test"
        };
        dbContext.PaymentRecords.Add(payment);
        dbContext.SaveChanges();

        service = new StripeService(
            dbContext,
            new Mock<IGetOrderInfoClient>().Object,
            new ListLogger<StripeService>(),
            Options.Create(new CardSettings { ApiKey = "sk_test_fake", WebhookSecret = WEBHOOK_SECRET }),
            new Mock<IUserClaimsExtractor>().Object,
            publishMock.Object,
            scorerMock.Object,
            disputeMock.Object);
    }

    private string CompletedPayload() => StripeWebhookSigner.Event("checkout.session.completed",
        "{\"id\":\"cs_test\",\"object\":\"checkout.session\",\"payment_intent\":\"pi_test\"," +
        "\"metadata\":{\"payment_id\":\"" + payment.Id + "\",\"order_id\":\"" + payment.OrderId + "\"}}");

    private static string DisputePayload(string type) => StripeWebhookSigner.Event(type,
        "{\"id\":\"dp_test\",\"object\":\"dispute\",\"charge\":\"ch_test\",\"payment_intent\":\"pi_test\"," +
        "\"created\":1730000000,\"status\":\"needs_response\",\"amount\":5000,\"currency\":\"usd\"}");

    [Fact]
    public async Task CheckoutCompleted_WhenFraudServiceIsDown_PaymentIsStillPaidAndPublished()
    {
        scorerMock.Setup(s => s.ScorePaidSessionAsync(It.IsAny<Session>(), It.IsAny<PaymentRecordEF>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FraudScoreOutcome.NotScored);
        string payload = CompletedPayload();

        await service.HandleWebhookAsync(payload, StripeWebhookSigner.Sign(payload, WEBHOOK_SECRET), CancellationToken.None);

        (await dbContext.PaymentRecords.SingleAsync()).Status.Should().Be(PaymentStatus.Paid);
        publishMock.Verify(p => p.Publish(It.Is<OrderPaidEvent>(e => e.OrderId == payment.OrderId), It.IsAny<CancellationToken>()), Times.Once);
        scorerMock.Verify(s => s.ScorePaidSessionAsync(
            It.Is<Session>(x => x.PaymentIntentId == "pi_test"),
            It.Is<PaymentRecordEF>(p => p.Id == payment.Id),
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckoutCompleted_RepeatedForDisputedPayment_DoesNotTurnItBackIntoPaid()
    {
        payment.Status = PaymentStatus.Disputed;
        await dbContext.SaveChangesAsync();
        string payload = CompletedPayload();

        await service.HandleWebhookAsync(payload, StripeWebhookSigner.Sign(payload, WEBHOOK_SECRET), CancellationToken.None);

        (await dbContext.PaymentRecords.SingleAsync()).Status.Should().Be(PaymentStatus.Disputed);
        publishMock.Verify(p => p.Publish(It.IsAny<OrderPaidEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        scorerMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("charge.dispute.created")]
    [InlineData("charge.dispute.closed")]
    public async Task DisputeEvent_WithValidSignature_IsPassedToDisputeHandler(string type)
    {
        string payload = DisputePayload(type);

        await service.HandleWebhookAsync(payload, StripeWebhookSigner.Sign(payload, WEBHOOK_SECRET), CancellationToken.None);

        disputeMock.Verify(d => d.HandleAsync(It.Is<Stripe.Event>(e => e.Type == type && ((Dispute)e.Data.Object).PaymentIntentId == "pi_test"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("charge.dispute.created")]
    [InlineData("charge.dispute.closed")]
    public async Task DisputeEvent_WithInvalidSignature_IsNotProcessed(string type)
    {
        string payload = DisputePayload(type);

        Func<Task> wrongSecret = () => service.HandleWebhookAsync(payload, StripeWebhookSigner.Sign(payload, "whsec_wrong"), CancellationToken.None);
        Func<Task> noSignature = () => service.HandleWebhookAsync(payload, "", CancellationToken.None);

        await wrongSecret.Should().NotThrowAsync();
        await noSignature.Should().NotThrowAsync();
        disputeMock.VerifyNoOtherCalls();
        (await dbContext.PaymentRecords.SingleAsync()).Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task DisputeEvent_WithTamperedPayload_IsNotProcessed()
    {
        string payload = DisputePayload("charge.dispute.created");
        string signature = StripeWebhookSigner.Sign(payload, WEBHOOK_SECRET);

        await service.HandleWebhookAsync(payload.Replace("pi_test", "pi_other"), signature, CancellationToken.None);

        disputeMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnknownEventType_IsIgnored()
    {
        string payload = StripeWebhookSigner.Event("charge.refunded", "{\"id\":\"ch_test\",\"object\":\"charge\"}");

        Func<Task> act = () => service.HandleWebhookAsync(payload, StripeWebhookSigner.Sign(payload, WEBHOOK_SECRET), CancellationToken.None);

        await act.Should().NotThrowAsync();
        disputeMock.VerifyNoOtherCalls();
        scorerMock.VerifyNoOtherCalls();
        publishMock.VerifyNoOtherCalls();
    }
}
