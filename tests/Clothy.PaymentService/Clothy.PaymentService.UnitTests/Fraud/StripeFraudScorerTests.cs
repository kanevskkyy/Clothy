using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.UnitTests.Infrastructure;
using FluentAssertions;
using Moq;
using Stripe;
using Stripe.Checkout;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Fraud;

public class StripeFraudScorerTests
{
    private readonly Mock<IFraudScoringClient> clientMock = new();
    private readonly Mock<IStripePaymentDetailsProvider> detailsMock = new();
    private readonly ListLogger<StripeFraudScorer> logger = new();

    private readonly PaymentRecordEF payment = new()
    {
        Id = Guid.NewGuid(),
        OrderId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Price = 120m,
        Status = PaymentStatus.Paid
    };

    private StripeFraudScorer CreateScorer() => new(clientMock.Object, detailsMock.Object, logger);

    private static Session PaidSession() => new()
    {
        Id = "cs_test",
        PaymentIntentId = "pi_test",
        CustomerDetails = new SessionCustomerDetails { Email = "person@ukr.net" },
        Metadata = new Dictionary<string, string>
        {
            { "device_type", "desktop" },
            { "device_os", "Windows 10" }
        }
    };

    [Fact]
    public async Task ScorePaidSessionAsync_BuildsRequestFromStripeData()
    {
        DateTime chargeCreated = new DateTime(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);
        detailsMock.Setup(d => d.GetChargeDetailsAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeChargeDetails("fp_1", "mastercard", "credit", "79000", null, chargeCreated));
        FraudScoreRequest? sent = null;
        clientMock.Setup(c => c.ScoreAsync(It.IsAny<FraudScoreRequest>(), It.IsAny<CancellationToken>()))
            .Callback<FraudScoreRequest, CancellationToken>((r, _) => sent = r)
            .ReturnsAsync(new FraudScoreResult(FraudScoreOutcome.Scored, new FraudScoreResponse { Flag = false }));

        FraudScoreOutcome outcome = await CreateScorer().ScorePaidSessionAsync(PaidSession(), payment, DateTime.UtcNow, CancellationToken.None);

        outcome.Should().Be(FraudScoreOutcome.Scored);
        sent!.TxId.Should().Be(payment.OrderId.ToString());
        sent.Ts.Should().Be(new DateTimeOffset(chargeCreated).ToUnixTimeSeconds());
        sent.TransactionAmt.Should().Be(120d);
        sent.Card1.Should().Be(FraudRequestMapper.CardFingerprintToCard1("fp_1"));
        sent.Card4.Should().Be("mastercard");
        sent.Card6.Should().Be("credit");
        sent.Addr1.Should().Be(79000d);
        sent.PEmailDomain.Should().Be("ukr.net");
        sent.DeviceType.Should().Be("desktop");
        sent.Id30.Should().Be("Windows 10");
        sent.Id31.Should().BeNull();
    }

    [Fact]
    public async Task ScorePaidSessionAsync_WhenFraudServiceIsDown_ReturnsNotScoredAndWarns()
    {
        detailsMock.Setup(d => d.GetChargeDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeChargeDetails("fp_1", "visa", "debit", null, null, DateTime.UtcNow));
        clientMock.Setup(c => c.ScoreAsync(It.IsAny<FraudScoreRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FraudScoreResult(FraudScoreOutcome.NotScored));

        FraudScoreOutcome outcome = await CreateScorer().ScorePaidSessionAsync(PaidSession(), payment, DateTime.UtcNow, CancellationToken.None);

        outcome.Should().Be(FraudScoreOutcome.NotScored);
        logger.Entries.Should().Contain(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
    }

    [Fact]
    public async Task ScorePaidSessionAsync_WhenStripeLookupThrows_DoesNotThrowAndDoesNotCallFraudService()
    {
        detailsMock.Setup(d => d.GetChargeDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("api down"));

        Func<Task<FraudScoreOutcome>> act = () => CreateScorer().ScorePaidSessionAsync(PaidSession(), payment, DateTime.UtcNow, CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.Should().Be(FraudScoreOutcome.NotScored);
        clientMock.Verify(c => c.ScoreAsync(It.IsAny<FraudScoreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScorePaidSessionAsync_WithoutCardFingerprint_ReturnsNotScored()
    {
        detailsMock.Setup(d => d.GetChargeDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeChargeDetails(null, null, null, null, null, null));

        FraudScoreOutcome outcome = await CreateScorer().ScorePaidSessionAsync(PaidSession(), payment, DateTime.UtcNow, CancellationToken.None);

        outcome.Should().Be(FraudScoreOutcome.NotScored);
        clientMock.Verify(c => c.ScoreAsync(It.IsAny<FraudScoreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ScorePaidSessionAsync_DoesNotLogPersonalData()
    {
        detailsMock.Setup(d => d.GetChargeDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeChargeDetails("fp_SECRET", "visa", "debit", "79000", "bill@ukr.net", DateTime.UtcNow));
        clientMock.Setup(c => c.ScoreAsync(It.IsAny<FraudScoreRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FraudScoreResult(FraudScoreOutcome.Scored, new FraudScoreResponse { Flag = true }));

        await CreateScorer().ScorePaidSessionAsync(PaidSession(), payment, DateTime.UtcNow, CancellationToken.None);

        logger.AllText.Should().NotContain("fp_SECRET").And.NotContain("ukr.net").And.NotContain("79000");
    }
}
