using System.Net;
using System.Text;
using System.Text.Json;

namespace Clothy.AuthService.UnitTests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private object responseBody;
    private HttpStatusCode statusCode;

    public MockHttpMessageHandler(object responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        this.responseBody = responseBody;
        this.statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(responseBody);
        HttpResponseMessage response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}