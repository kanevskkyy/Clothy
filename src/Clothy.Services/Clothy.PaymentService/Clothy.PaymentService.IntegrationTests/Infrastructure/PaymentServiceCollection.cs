using Xunit;

namespace Clothy.PaymentService.IntegrationTests.Infrastructure;

[CollectionDefinition("PaymentService")]
public class PaymentServiceCollection : ICollectionFixture<PaymentServiceWebApplicationFactory>
{
    
}