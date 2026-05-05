using Xunit;

namespace Clothy.CatalogService.ContractTests.Fixtures;

[CollectionDefinition("CatalogContract")]
public class CatalogContractCollectionFixture : ICollectionFixture<Infrastructure.CatalogGrpcWebFactory>
{
    
}