using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.UnitTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Services;

/// <summary>
/// checkout.session.completed and charge.dispute.created in any order, in parallel and delivered twice.
/// Always: payment Disputed, OrderPaidEvent once, /score once, /labels once and after /score.
/// </summary>
public class StripeWebhookOrderingTests
{
    private static async Task AssertDisputedOnceAsync(PaymentWebhookHarness h)
    {
        PaymentRecordEF stored = await h.StoredPaymentAsync();
        stored.Status.Should().Be(PaymentStatus.Disputed);
        stored.FraudSync.Should().Be(FraudSyncState.Labeled);
        stored.DisputedAt.Should().Be(PaymentWebhookHarness.DisputeCreatedUtc);

        h.PublishedOrderPaidEvents.Should().Be(1);
        h.Fraud.Count("score").Should().Be(1);
        h.Fraud.Count("label:true").Should().Be(1);
        h.Fraud.Count("label-404").Should().Be(0, "a label must never be sent before /score");
        h.Fraud.Calls.Should().Equal("score", "label:true");
        h.Fraud.LabelTimes.Should().Equal(new DateTimeOffset(PaymentWebhookHarness.DisputeCreatedUtc).ToUnixTimeSeconds());
    }

    [Fact]
    public async Task CheckoutThenDispute()
    {
        PaymentWebhookHarness h = new();

        await h.SendAsync(h.CheckoutCompleted());
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        await AssertDisputedOnceAsync(h);
    }

    [Fact]
    public async Task DisputeThenCheckout()
    {
        // the reported race: the dispute arrives while the payment is still Pending
        PaymentWebhookHarness h = new();

        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());
        await h.SendAsync(h.CheckoutCompleted());

        await AssertDisputedOnceAsync(h);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EachEventDeliveredTwice(bool checkoutFirst)
    {
        PaymentWebhookHarness h = new();
        string checkout = h.CheckoutCompleted();
        string dispute = PaymentWebhookHarness.DisputeCreated();

        if (checkoutFirst)
        {
            await h.SendAsync(checkout);
            await h.SendAsync(checkout);
            await h.SendAsync(dispute);
            await h.SendAsync(dispute);
        }
        else
        {
            await h.SendAsync(dispute);
            await h.SendAsync(dispute);
            await h.SendAsync(checkout);
            await h.SendAsync(checkout);
        }

        await AssertDisputedOnceAsync(h);
    }

    [Fact]
    public async Task RepeatedCheckoutAfterDispute_ChangesNothing()
    {
        PaymentWebhookHarness h = new();
        await h.SendAsync(h.CheckoutCompleted());
        await h.SendAsync(PaymentWebhookHarness.DisputeCreated());

        await h.SendAsync(h.CheckoutCompleted());

        await AssertDisputedOnceAsync(h);
    }

    /// <summary>
    /// Both webhooks at the same time, several times each, with a slow /score so that the dispute
    /// lands while scoring is still running. Repeated to give the scheduler many interleavings.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(40)]
    public async Task ParallelDelivery(int scoreDelayMs)
    {
        for (int run = 0; run < 15; run++)
        {
            PaymentWebhookHarness h = new();
            h.Fraud.ScoreDelay = TimeSpan.FromMilliseconds(scoreDelayMs);
            string checkout = h.CheckoutCompleted();
            string dispute = PaymentWebhookHarness.DisputeCreated();
            using Barrier barrier = new Barrier(4);

            Task Send(string payload) => Task.Run(async () =>
            {
                barrier.SignalAndWait();
                await h.SendAsync(payload);
            });

            await Task.WhenAll(Send(checkout), Send(dispute), Send(checkout), Send(dispute));

            await AssertDisputedOnceAsync(h);
        }
    }

    [Fact]
    public async Task DisputeWonAfterRace_RemovesLabelOnce()
    {
        PaymentWebhookHarness h = new();
        h.Fraud.ScoreDelay = TimeSpan.FromMilliseconds(20);
        await Task.WhenAll(
            Task.Run(() => h.SendAsync(PaymentWebhookHarness.DisputeCreated())),
            Task.Run(() => h.SendAsync(h.CheckoutCompleted())));

        string won = PaymentWebhookHarness.DisputeClosed("won");
        await h.SendAsync(won);
        await h.SendAsync(won);

        (await h.StoredPaymentAsync()).Status.Should().Be(PaymentStatus.DisputeWon);
        h.Fraud.Calls.Should().Equal("score", "label:true", "label:false");
        h.PublishedOrderPaidEvents.Should().Be(1);
    }
}
