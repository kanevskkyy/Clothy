using System.Collections.Concurrent;
using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.BLL.Services;
using Clothy.PaymentService.DAL.Context;
using Clothy.PaymentService.Domain.Entities;
using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Clothy.Shared.Events.PaymentEvents;
using Clothy.Shared.Helpers.JWT;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Moq;

namespace Clothy.PaymentService.UnitTests.Infrastructure;

/// <summary>
/// FraudService stand-in that behaves like the real one where it matters for event ordering:
/// /labels for a txid that was never scored answers 404.
/// </summary>
public class FakeFraudService : IFraudScoringClient
{
    private readonly ConcurrentDictionary<string, bool> scored = new();
    private readonly object sync = new();

    public TimeSpan ScoreDelay { get; set; } = TimeSpan.Zero;

    /// <summary>Calls in the order they started: "score", "label:true", "label:false", "label-404".</summary>
    public List<string> Calls { get; } = new();

    public List<long> LabelTimes { get; } = new();

    private void Record(string call)
    {
        lock (sync) Calls.Add(call);
    }

    public int Count(string call)
    {
        lock (sync) return Calls.Count(c => c == call);
    }

    public async Task<FraudScoreResult> ScoreAsync(FraudScoreRequest request, CancellationToken cancellationToken)
    {
        Record("score");
        if (ScoreDelay > TimeSpan.Zero) await Task.Delay(ScoreDelay, cancellationToken);
        if (!scored.TryAdd(request.TxId, true)) return new FraudScoreResult(FraudScoreOutcome.AlreadyScored);
        return new FraudScoreResult(FraudScoreOutcome.Scored, new FraudScoreResponse { TxId = request.TxId, Score = 0.1 });
    }

    public Task<FraudLabelOutcome> LabelAsync(string txId, bool fraud, long atUnixSeconds, CancellationToken cancellationToken)
    {
        if (!scored.ContainsKey(txId))
        {
            Record("label-404");
            return Task.FromResult(FraudLabelOutcome.UnknownTransaction);
        }

        Record(fraud ? "label:true" : "label:false");
        lock (sync) LabelTimes.Add(atUnixSeconds);
        return Task.FromResult(FraudLabelOutcome.Labeled);
    }
}

/// <summary>
/// The real webhook pipeline (StripeService, lifecycle, dispute handler, scorer) over one shared in-memory
/// database. Every webhook gets its own DbContext and service graph, like a separate HTTP request.
/// </summary>
public class PaymentWebhookHarness
{
    public const string WEBHOOK_SECRET = "whsec_harness";
    public static readonly DateTime DisputeCreatedUtc = new(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);

    // one root for all tests: a new root per test makes EF build a new internal service provider each time
    private static readonly InMemoryDatabaseRoot databaseRoot = new();
    private readonly string databaseName = Guid.NewGuid().ToString();

    public Mock<IPublishEndpoint> Publish { get; } = new();
    public FakeFraudService Fraud { get; } = new();
    public Mock<IStripePaymentDetailsProvider> StripeDetails { get; } = new();
    public PaymentRecordEF Payment { get; }

    public PaymentWebhookHarness(PaymentStatus status = PaymentStatus.Pending, FraudSyncState fraudSync = FraudSyncState.None)
    {
        Payment = new PaymentRecordEF
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Price = 75m,
            Status = status,
            FraudSync = fraudSync,
            TransactionId = "cs_test",
            UpdatedAt = DateTime.UtcNow
        };
        using (PaymentDbContext db = CreateDbContext())
        {
            db.PaymentRecords.Add(Payment);
            db.SaveChanges();
        }

        StripeDetails.Setup(d => d.GetChargeDetailsAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeChargeDetails("fp_test", "visa", "debit", "01001", null, DateTime.UtcNow, "usd"));
        StripeDetails.Setup(d => d.GetPaymentIntentMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "payment_id", Payment.Id.ToString() },
                { "order_id", Payment.OrderId.ToString() }
            });
        StripeDetails.Setup(d => d.GetCheckoutSessionMetadataAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string> { { "payment_id", Payment.Id.ToString() }, { "device_type", "mobile" } });
    }

    public PaymentDbContext CreateDbContext() => new(new DbContextOptionsBuilder<PaymentDbContext>()
        .UseInMemoryDatabase(databaseName, databaseRoot).Options);

    public async Task<PaymentRecordEF> StoredPaymentAsync()
    {
        await using PaymentDbContext db = CreateDbContext();
        return await db.PaymentRecords.AsNoTracking().SingleAsync(p => p.Id == Payment.Id);
    }

    public int PublishedOrderPaidEvents => Publish.Invocations.Count(i =>
        i.Method.Name == nameof(IPublishEndpoint.Publish) && i.Arguments[0] is OrderPaidEvent e && e.OrderId == Payment.OrderId);

    /// <summary>Runs one signed webhook in its own scope.</summary>
    public async Task SendAsync(string payload)
    {
        await using PaymentDbContext db = CreateDbContext();
        PaymentRecordUpdater updater = new PaymentRecordUpdater(db);
        StripeFraudScorer scorer = new StripeFraudScorer(Fraud, StripeDetails.Object, new ListLogger<StripeFraudScorer>());
        StripePaymentLifecycle lifecycle = new StripePaymentLifecycle(updater, Publish.Object, scorer, Fraud, new ListLogger<StripePaymentLifecycle>());
        StripeDisputeHandler disputes = new StripeDisputeHandler(db, updater, lifecycle, StripeDetails.Object, Fraud, new ListLogger<StripeDisputeHandler>());
        StripeService service = new StripeService(
            db,
            new Mock<IGetOrderInfoClient>().Object,
            new ListLogger<StripeService>(),
            Options.Create(new CardSettings { ApiKey = "sk_test_fake", WebhookSecret = WEBHOOK_SECRET }),
            new Mock<IUserClaimsExtractor>().Object,
            lifecycle,
            disputes);

        await service.HandleWebhookAsync(payload, StripeWebhookSigner.Sign(payload, WEBHOOK_SECRET), CancellationToken.None);
    }

    public string CheckoutCompleted() => StripeWebhookSigner.Event("checkout.session.completed",
        "{\"id\":\"cs_test\",\"object\":\"checkout.session\",\"payment_intent\":\"pi_test\"," +
        "\"metadata\":{\"payment_id\":\"" + Payment.Id + "\",\"order_id\":\"" + Payment.OrderId + "\"}}");

    public static string DisputeCreated() => Dispute("charge.dispute.created", "needs_response");

    public static string DisputeClosed(string status) => Dispute("charge.dispute.closed", status);

    private static string Dispute(string type, string status) => StripeWebhookSigner.Event(type,
        "{\"id\":\"dp_test\",\"object\":\"dispute\",\"charge\":\"ch_test\",\"payment_intent\":\"pi_test\"," +
        "\"created\":" + new DateTimeOffset(DisputeCreatedUtc).ToUnixTimeSeconds() + ",\"status\":\"" + status +
        "\",\"amount\":7500,\"currency\":\"usd\"}",
        created: new DateTimeOffset(DisputeCreatedUtc).ToUnixTimeSeconds() + (type.EndsWith("closed") ? 86400 * 10 : 0));
}
