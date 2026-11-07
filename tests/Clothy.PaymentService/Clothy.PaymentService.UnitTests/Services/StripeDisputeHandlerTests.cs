using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.UnitTests.Infrastructure;
using FluentAssertions;
using Moq;
using Stripe;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Services;

/// <summary>Dispute events for a payment that was paid (and scored) earlier.</summary>
public class StripeDisputeHandlerTests
{
    private readonly PaymentWebhookHarness h;

    public StripeDisputeHandlerTests()
    {
        h = new PaymentWebhookHarness(PaymentStatus.Paid, FraudSyncState.Scored);
        // the order was scored when it was paid
        h.Fraud.ScoreAsync(new Clothy.PaymentService.BLL.Fraud.FraudScoreRequest { TxId = h.Payment.OrderId.ToString() }, CancellationToken.None).Wait();
        h.Fraud.Calls.Clear();
    }

    private async Task<PaymentStatus> StatusAsync() => (await h.StoredPaymentAsync()).Status;

    [Fact]
    public async Task Created_MarksPaymentDisputedAndLabelsOrderAsFraud()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        (await StatusAsync()).Should().Be(PaymentStatus.Disputed);
        h.Fraud.Calls.Should().Equal("label:true");
        h.Fraud.LabelTimes.Should().Equal(new DateTimeOffset(PaymentWebhookHarness.DisputeCreatedUtc).ToUnixTimeSeconds());
        h.PublishedOrderPaidEvents.Should().Be(0, "the payment was already paid");
    }

    [Fact]
    public async Task Created_WhenDeliveredTwice_LabelsOnlyOnce()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        (await StatusAsync()).Should().Be(PaymentStatus.Disputed);
        h.Fraud.Calls.Should().Equal("label:true");
    }

    [Fact]
    public async Task ClosedWon_MarksDisputeWonAndRemovesFraudLabel()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("won"));

        (await StatusAsync()).Should().Be(PaymentStatus.DisputeWon);
        h.Fraud.Calls.Should().Equal("label:true", "label:false");
    }

    [Fact]
    public async Task ClosedWon_WhenDeliveredTwice_RemovesLabelOnlyOnce()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("won"));
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("won"));

        h.Fraud.Calls.Should().Equal("label:true", "label:false");
    }

    [Fact]
    public async Task ClosedLost_MarksDisputeLostAndKeepsExistingLabel()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("lost"));

        (await StatusAsync()).Should().Be(PaymentStatus.DisputeLost);
        h.Fraud.Calls.Should().Equal("label:true");
    }

    [Fact]
    public async Task ClosedLost_WhenCreatedWasMissed_SetsFraudLabelWithDisputeTime()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("lost"));

        (await StatusAsync()).Should().Be(PaymentStatus.DisputeLost);
        h.Fraud.Calls.Should().Equal("label:true");
        h.Fraud.LabelTimes.Should().Equal(new DateTimeOffset(PaymentWebhookHarness.DisputeCreatedUtc).ToUnixTimeSeconds());
    }

    [Fact]
    public async Task Created_AfterClosed_DoesNotReopenDispute()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("won"));
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        (await StatusAsync()).Should().Be(PaymentStatus.DisputeWon);
        h.Fraud.Calls.Should().BeEmpty("no label was sent, so there is nothing to remove");
    }

    [Fact]
    public async Task ClosedWithOtherStatus_ChangesNothing()
    {
        await h.SendAsync(PaymentWebhookHarness.DisputeClosed("warning_closed"));

        (await StatusAsync()).Should().Be(PaymentStatus.Paid);
        h.Fraud.Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task Created_WhenFraudServiceDoesNotKnowTheOrder_StillSavesDisputedStatus()
    {
        // e.g. FraudService was down when the order was paid: /labels answers 404
        PaymentWebhookHarness unscored = new(PaymentStatus.Paid, FraudSyncState.Scored);

        Func<Task> act = () => unscored.SendAsync(PaymentWebhookHarness.DisputeCreated());

        await act.Should().NotThrowAsync();
        (await unscored.StoredPaymentAsync()).Status.Should().Be(PaymentStatus.Disputed);
        unscored.Fraud.Calls.Should().Equal("label-404");
    }

    [Fact]
    public async Task Created_ForOldPaymentWithoutIntentMetadata_FindsOrderThroughCheckoutSession()
    {
        h.StripeDetails.Setup(d => d.GetPaymentIntentMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string>());
        h.StripeDetails.Setup(d => d.GetCheckoutSessionMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string> { { "order_id", h.Payment.OrderId.ToString() } });

        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        (await StatusAsync()).Should().Be(PaymentStatus.Disputed);
    }

    [Fact]
    public async Task Created_ForUnknownPayment_DoesNothing()
    {
        h.StripeDetails.Setup(d => d.GetPaymentIntentMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("No such payment_intent"));
        h.StripeDetails.Setup(d => d.GetCheckoutSessionMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyDictionary<string, string>?)null);

        Func<Task> act = () => h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        await act.Should().NotThrowAsync();
        (await StatusAsync()).Should().Be(PaymentStatus.Paid);
        h.Fraud.Calls.Should().BeEmpty();
    }
}
