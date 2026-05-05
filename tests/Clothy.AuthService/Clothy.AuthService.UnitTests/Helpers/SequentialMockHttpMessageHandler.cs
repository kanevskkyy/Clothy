using System.Net;
using System.Text;
using System.Text.Json;

namespace Clothy.AuthService.UnitTests.Helpers;

public class SequentialMockHttpMessageHandler : HttpMessageHandler
{
    private Queue<(object body, HttpStatusCode status)> responses;

    public SequentialMockHttpMessageHandler(
        params (object body, HttpStatusCode status)[] responses)
    {
        this.responses = new Queue<(object, HttpStatusCode)>(responses);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!responses.TryDequeue(out var response)) throw new InvalidOperationException("No more responses configured");

        string json = JsonSerializer.Serialize(response.body);
        return Task.FromResult(new HttpResponseMessage(response.status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}