using System.Net;
using System.Text;
using System.Text.Json;

namespace Clothy.PaymentService.IntegrationTests.Infrastructure;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private string clientName;
 
    public MockHttpMessageHandler(string clientName)
    {
        this.clientName = clientName;
    }
 
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = clientName switch
        {
            "NowPayments" => BuildNowPaymentsResponse(),
            _ => BuildStripeResponse()
        };
 
        return Task.FromResult(response);
    }
 
    private static HttpResponseMessage BuildNowPaymentsResponse()
    {
        string body = JsonSerializer.Serialize(new
        {
            id = "nowpay_test_123",
            invoice_url = "https://nowpayments.io/payment/test-invoice-url"
        });
 
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
 
    private static HttpResponseMessage BuildStripeResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
    }
}