using System.Net;

namespace Clothy.AuthService.UnitTests.Helpers;

public static class MockHttpHelper
{
    public static HttpClient CreateHttpClientWithResponse(
        object responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        HttpMethod? expectedMethod = null,
        string? expectedUrl = null)
    {
        MockHttpMessageHandler handler = new MockHttpMessageHandler(responseBody, statusCode);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }

    public static HttpClient CreateHttpClientWithSequence(
        params (object body, HttpStatusCode status)[] responses)
    {
        SequentialMockHttpMessageHandler handler = new SequentialMockHttpMessageHandler(responses);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }
}