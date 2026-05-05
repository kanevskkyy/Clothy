using Xunit;

namespace Clothy.BasketService.ContractTests.Fixtures;

[CollectionDefinition("BasketContract")]
public class BasketContractCollectionFixture : ICollectionFixture<Infrastructure.BasketGrpcWebFactory>
{
    
}
