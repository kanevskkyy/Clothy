using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Clothy.PaymentService.UnitTests.Infrastructure;

/// <summary>HttpMessageHandler that answers with a delegate and records every request.</summary>
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler;

    public List<(HttpMethod Method, string Path, string? Body)> Requests { get; } = new();

    public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        this.handler = handler;
    }

    public static StubHttpMessageHandler Returning(HttpStatusCode status, string body = "{}")
    {
        return new StubHttpMessageHandler((_, _) => Task.FromResult(Json(status, body)));
    }

    public static HttpResponseMessage Json(HttpStatusCode status, string body = "{}")
    {
        return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request.Method, request.RequestUri!.AbsolutePath, body));
        return await handler(request, cancellationToken);
    }
}

/// <summary>Logger that keeps formatted messages, to check what ends up in the logs.</summary>
public class ListLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Add((logLevel, formatter(state, exception)));
    }

    public string AllText => string.Join("\n", Entries.Select(e => e.Message));
}

/// <summary>Builds Stripe webhook payloads signed the same way Stripe does (t=...,v1=HMAC-SHA256).</summary>
public static class StripeWebhookSigner
{
    public static string Sign(string payload, string secret)
    {
        long t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{t}.{payload}"));
        return $"t={t},v1={Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    public static string Event(string type, string dataObjectJson, long created = 1730000000)
    {
        return "{\"id\":\"evt_test\",\"object\":\"event\",\"api_version\":\"" + StripeConfiguration.ApiVersion +
               "\",\"created\":" + created + ",\"livemode\":false,\"pending_webhooks\":1," +
               "\"request\":{\"id\":null,\"idempotency_key\":null},\"type\":\"" + type +
               "\",\"data\":{\"object\":" + dataObjectJson + "}}";
    }
}
