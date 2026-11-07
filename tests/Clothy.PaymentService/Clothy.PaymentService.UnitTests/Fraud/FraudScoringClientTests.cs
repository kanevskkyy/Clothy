using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Clothy.PaymentService.BLL.Config;
using Clothy.PaymentService.BLL.Fraud;
using Clothy.PaymentService.UnitTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Clothy.PaymentService.UnitTests.Fraud;

public class FraudScoringClientTests
{
    private const string SCORE_RESPONSE = """
        {"txid":"o1","score":0.52,"flag":true,"threshold":0.4008,"flag_rate_val":0.01,"variant":"main",
         "ring":{"suspect":false,"active":true,"other_cards_30d":null,"share_new":null,"k":2,"s":0.5},
         "top_features":[{"feature":"card_n_1d","value":3.0,"contribution":0.9},
                         {"feature":"addr1","value":null,"contribution":0.2},
                         {"feature":"hour","value":4.0,"contribution":-0.1}]}
        """;

    private readonly ListLogger<FraudScoringClient> logger = new();

    private FraudScoringClient CreateClient(StubHttpMessageHandler handler, TimeSpan? timeout = null)
    {
        HttpClient http = new HttpClient(handler) { BaseAddress = new Uri("http://fraud-service:8097/") };
        IOptions<FraudSettings> settings = Options.Create(new FraudSettings { Timeout = timeout ?? TimeSpan.FromSeconds(2) });
        return new FraudScoringClient(http, logger, settings);
    }

    private static FraudScoreRequest Request() => FraudRequestMapper.Build(new FraudPaymentData(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        new DateTime(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc),
        49.90m,
        "fp_SECRET_FINGERPRINT",
        "visa",
        "debit",
        "01001",
        "buyer.private@gmail.com",
        null));

    [Fact]
    public async Task ScoreAsync_WhenServiceAnswers200_ReturnsScoredWithDecision()
    {
        StubHttpMessageHandler handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, SCORE_RESPONSE);

        FraudScoreResult result = await CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        result.Outcome.Should().Be(FraudScoreOutcome.Scored);
        result.Response!.Flag.Should().BeTrue();
        result.Response.Score.Should().Be(0.52);
        result.Response.TopFeatures.Should().HaveCount(3);
        result.Response.TopFeatures[0].Feature.Should().Be("card_n_1d");

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].Path.Should().Be("/score");
    }

    [Fact]
    public async Task ScoreAsync_SendsIeeeCisFieldNamesAndOmitsNulls()
    {
        StubHttpMessageHandler handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, SCORE_RESPONSE);

        await CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        using JsonDocument body = JsonDocument.Parse(handler.Requests[0].Body!);
        JsonElement root = body.RootElement;
        root.GetProperty("txid").GetString().Should().Be("11111111-1111-1111-1111-111111111111");
        root.GetProperty("ts").GetInt64().Should().Be(new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
        root.GetProperty("TransactionAmt").GetDouble().Should().Be(49.9);
        root.GetProperty("ProductCD").GetString().Should().Be("W");
        root.GetProperty("card4").GetString().Should().Be("visa");
        root.GetProperty("card6").GetString().Should().Be("debit");
        root.GetProperty("addr1").GetDouble().Should().Be(1001);
        root.GetProperty("P_emaildomain").GetString().Should().Be("gmail.com");
        root.TryGetProperty("DeviceType", out _).Should().BeFalse();
        root.TryGetProperty("id_31", out _).Should().BeFalse();
        // the request carries only the email domain, never the address itself or the raw fingerprint
        handler.Requests[0].Body.Should().NotContain("buyer.private").And.NotContain("fp_SECRET_FINGERPRINT");
    }

    [Fact]
    public async Task ScoreAsync_WhenServiceDoesNotAnswerInTime_ReturnsNotScoredWithinTimeout()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return StubHttpMessageHandler.Json(HttpStatusCode.OK);
        });
        Stopwatch watch = Stopwatch.StartNew();

        FraudScoreResult result = await CreateClient(handler, TimeSpan.FromMilliseconds(200)).ScoreAsync(Request(), CancellationToken.None);

        watch.Stop();
        result.Outcome.Should().Be(FraudScoreOutcome.NotScored);
        watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        logger.Entries.Should().Contain(e => e.Level == LogLevel.Warning && e.Message.Contains("did not answer"));
    }

    [Fact]
    public void DefaultTimeoutIsTwoSeconds()
    {
        new FraudSettings().Timeout.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ScoreAsync_When409AndTransactionExists_ReturnsAlreadyScored()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler((req, _) => Task.FromResult(
            req.Method == HttpMethod.Post
                ? StubHttpMessageHandler.Json(HttpStatusCode.Conflict, "{\"detail\":\"транзакція уже збережена\"}")
                : StubHttpMessageHandler.Json(HttpStatusCode.OK, "{\"txid\":\"x\",\"score\":0.1,\"flag\":false}")));

        FraudScoreResult result = await CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        result.Outcome.Should().Be(FraudScoreOutcome.AlreadyScored);
        handler.Requests.Select(r => (r.Method, r.Path)).Should().Equal(
            (HttpMethod.Post, "/score"),
            (HttpMethod.Get, "/tx/11111111-1111-1111-1111-111111111111"));
        logger.Entries.Should().NotContain(e => e.Level >= LogLevel.Warning);
    }

    [Fact]
    public async Task ScoreAsync_When409ButTransactionIsUnknown_ReturnsNotScored()
    {
        // 409 for a late webhook: ts is earlier than the last stored transaction, nothing was saved
        StubHttpMessageHandler handler = new StubHttpMessageHandler((req, _) => Task.FromResult(
            req.Method == HttpMethod.Post
                ? StubHttpMessageHandler.Json(HttpStatusCode.Conflict, "{\"detail\":\"ts раніше за останній\"}")
                : StubHttpMessageHandler.Json(HttpStatusCode.NotFound, "{\"detail\":\"невідома транзакція\"}")));

        FraudScoreResult result = await CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        result.Outcome.Should().Be(FraudScoreOutcome.NotScored);
        logger.Entries.Should().Contain(e => e.Level == LogLevel.Warning);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    public async Task ScoreAsync_WhenServiceFails_ReturnsNotScoredWithoutThrowing(HttpStatusCode status)
    {
        StubHttpMessageHandler handler = StubHttpMessageHandler.Returning(status, "{\"detail\":\"boom\"}");

        Func<Task<FraudScoreResult>> act = () => CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        (await act.Should().NotThrowAsync()).Which.Outcome.Should().Be(FraudScoreOutcome.NotScored);
        logger.Entries.Should().Contain(e => e.Level == LogLevel.Warning && e.Message.Contains(((int)status).ToString()));
    }

    [Fact]
    public async Task ScoreAsync_WhenServiceIsUnreachable_ReturnsNotScored()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("Connection refused (fraud-service:8097)"));

        FraudScoreResult result = await CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        result.Outcome.Should().Be(FraudScoreOutcome.NotScored);
    }

    [Fact]
    public async Task ScoreAsync_DoesNotLogPersonalData()
    {
        StubHttpMessageHandler handler = StubHttpMessageHandler.Returning(HttpStatusCode.InternalServerError);

        await CreateClient(handler).ScoreAsync(Request(), CancellationToken.None);

        logger.AllText.Should().NotContain("fp_SECRET_FINGERPRINT")
            .And.NotContain("buyer.private")
            .And.NotContain("gmail.com")
            .And.NotContain("01001");
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, FraudLabelOutcome.Labeled)]
    [InlineData(HttpStatusCode.NotFound, FraudLabelOutcome.UnknownTransaction)]
    [InlineData(HttpStatusCode.InternalServerError, FraudLabelOutcome.Failed)]
    public async Task LabelAsync_MapsStatusToOutcome(HttpStatusCode status, FraudLabelOutcome expected)
    {
        StubHttpMessageHandler handler = StubHttpMessageHandler.Returning(status, "{\"ok\":true}");

        FraudLabelOutcome outcome = await CreateClient(handler).LabelAsync("order-1", true, 1730000000, CancellationToken.None);

        outcome.Should().Be(expected);
        handler.Requests[0].Path.Should().Be("/labels");
        using JsonDocument body = JsonDocument.Parse(handler.Requests[0].Body!);
        body.RootElement.GetProperty("txid").GetString().Should().Be("order-1");
        body.RootElement.GetProperty("fraud").GetBoolean().Should().BeTrue();
        body.RootElement.GetProperty("at").GetInt64().Should().Be(1730000000);
    }

    [Fact]
    public async Task LabelAsync_WhenServiceIsUnreachable_ReturnsFailed()
    {
        StubHttpMessageHandler handler = new StubHttpMessageHandler((_, _) => throw new HttpRequestException("refused"));

        FraudLabelOutcome outcome = await CreateClient(handler).LabelAsync("order-1", false, 1, CancellationToken.None);

        outcome.Should().Be(FraudLabelOutcome.Failed);
    }
}
