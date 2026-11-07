using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.BLL.Services;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.UnitTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stripe;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Services;

public class StripeDisputeHandlerTests
{
    private static readonly DateTime DisputeCreated = new(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ClosedEventCreated = new(2026, 12, 5, 9, 0, 0, DateTimeKind.Utc);

    private readonly PaymentDbContext dbContext;
    private readonly Mock<IStripePaymentDetailsProvider> detailsMock = new();
    private readonly Mock<IFraudScoringClient> fraudClientMock = new();
    private readonly StripeDisputeHandler handler;
    private readonly PaymentRecordEF payment;

    public StripeDisputeHandlerTests()
    {
        dbContext = new PaymentDbContext(new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        payment = new PaymentRecordEF
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Price = 100m,
            Status = PaymentStatus.Paid,
            TransactionId = "cs_test",
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.PaymentRecords.Add(payment);
        dbContext.SaveChanges();

        detailsMock.Setup(d => d.GetPaymentIntentMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "payment_id", payment.Id.ToString() },
                { "order_id", payment.OrderId.ToString() }
            });
        fraudClientMock.Setup(c => c.LabelAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FraudLabelOutcome.Labeled);

        handler = new StripeDisputeHandler(dbContext, detailsMock.Object, fraudClientMock.Object, new ListLogger<StripeDisputeHandler>());
    }

    private static Event DisputeEvent(string type, string status, string paymentIntentId = "pi_test") => new()
    {
        Id = "evt_" + Guid.NewGuid().ToString("N"),
        Type = type,
        Created = type == StripeDisputeHandler.DISPUTE_CLOSED ? ClosedEventCreated : DisputeCreated,
        Data = new EventData
        {
            Object = new Dispute
            {
                Id = "dp_test",
                PaymentIntentId = paymentIntentId,
                ChargeId = "ch_test",
                Created = DisputeCreated,
                Status = status
            }
        }
    };

    private static long Unix(DateTime utc) => new DateTimeOffset(utc).ToUnixTimeSeconds();

    private async Task<PaymentStatus> StoredStatus() =>
        (await dbContext.PaymentRecords.AsNoTracking().SingleAsync(p => p.Id == payment.Id)).Status;

    [Fact]
    public async Task Created_MarksPaymentDisputedAndLabelsOrderAsFraud()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.Disputed);
        fraudClientMock.Verify(c => c.LabelAsync(payment.OrderId.ToString(), true, Unix(DisputeCreated), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Created_WhenDeliveredTwice_LabelsOnlyOnce()
    {
        Event e = DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response");

        await handler.HandleAsync(e, CancellationToken.None);
        await handler.HandleAsync(e, CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.Disputed);
        fraudClientMock.Verify(c => c.LabelAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClosedWon_MarksDisputeWonAndRemovesFraudLabel()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response"), CancellationToken.None);
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CLOSED, "won"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.DisputeWon);
        fraudClientMock.Verify(c => c.LabelAsync(payment.OrderId.ToString(), false, Unix(ClosedEventCreated), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClosedWon_WhenDeliveredTwice_RemovesLabelOnlyOnce()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response"), CancellationToken.None);
        Event closed = DisputeEvent(StripeDisputeHandler.DISPUTE_CLOSED, "won");

        await handler.HandleAsync(closed, CancellationToken.None);
        await handler.HandleAsync(closed, CancellationToken.None);

        fraudClientMock.Verify(c => c.LabelAsync(It.IsAny<string>(), false, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClosedLost_MarksDisputeLostAndKeepsExistingLabel()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response"), CancellationToken.None);
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CLOSED, "lost"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.DisputeLost);
        fraudClientMock.Verify(c => c.LabelAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClosedLost_WhenCreatedWasMissed_SetsFraudLabelWithDisputeTime()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CLOSED, "lost"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.DisputeLost);
        fraudClientMock.Verify(c => c.LabelAsync(payment.OrderId.ToString(), true, Unix(DisputeCreated), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Created_AfterClosed_DoesNotReopenDispute()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CLOSED, "won"), CancellationToken.None);
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.DisputeWon);
    }

    [Fact]
    public async Task ClosedWithOtherStatus_ChangesNothing()
    {
        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CLOSED, "warning_closed"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.Paid);
        fraudClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Created_WhenFraudServiceFails_StillSavesDisputedStatus()
    {
        fraudClientMock.Setup(c => c.LabelAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FraudLabelOutcome.Failed);

        Func<Task> act = () => handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        (await StoredStatus()).Should().Be(PaymentStatus.Disputed);
    }

    [Fact]
    public async Task Created_ForOldPaymentWithoutIntentMetadata_FindsOrderThroughCheckoutSession()
    {
        detailsMock.Setup(d => d.GetPaymentIntentMetadataAsync("pi_old", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string>());
        detailsMock.Setup(d => d.GetCheckoutSessionMetadataAsync("pi_old", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string> { { "order_id", payment.OrderId.ToString() } });

        await handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response", "pi_old"), CancellationToken.None);

        (await StoredStatus()).Should().Be(PaymentStatus.Disputed);
    }

    [Fact]
    public async Task Created_ForUnknownPayment_DoesNothing()
    {
        detailsMock.Setup(d => d.GetPaymentIntentMetadataAsync("pi_other", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("No such payment_intent"));

        Func<Task> act = () => handler.HandleAsync(DisputeEvent(StripeDisputeHandler.DISPUTE_CREATED, "needs_response", "pi_other"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        (await StoredStatus()).Should().Be(PaymentStatus.Paid);
        fraudClientMock.VerifyNoOtherCalls();
    }
}
