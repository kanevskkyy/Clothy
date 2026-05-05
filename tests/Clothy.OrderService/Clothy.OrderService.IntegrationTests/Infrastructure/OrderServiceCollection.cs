using Xunit;

namespace Clothy.OrderService.IntegrationTests.Infrastructure;

[CollectionDefinition("OrderService")]
public class OrderServiceCollection : ICollectionFixture<OrderServiceWebApplicationFactory>
{
    
}