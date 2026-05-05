using Xunit;

namespace Clothy.OrderService.ContractTests.Fixtures;

[CollectionDefinition("OrderContract")]
public class OrderContractCollectionFixture : ICollectionFixture<Infrastructure.OrderGrpcWebFactory>
{
    
}
