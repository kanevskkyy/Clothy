namespace Clothy.PaymentService.IntegrationTests.Infrastructure;

public class MockHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        MockHttpMessageHandler handler = new MockHttpMessageHandler(name);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://mock-payment-api.test/")
        };
    }
}