using Xunit;

namespace Clothy.CatalogService.IntegrationTests.Infrastructure;

[CollectionDefinition("CatalogService")]
public class CatalogServiceCollection : ICollectionFixture<CatalogServiceWebApplicationFactory>
{
    
}