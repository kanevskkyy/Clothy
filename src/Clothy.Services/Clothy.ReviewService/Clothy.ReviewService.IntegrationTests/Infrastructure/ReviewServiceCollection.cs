using Xunit;

namespace Clothy.ReviewService.IntegrationTests.Infrastructure;

[CollectionDefinition("ReviewService")]
public class ReviewServiceCollection : ICollectionFixture<ReviewServiceWebApplicationFactory>
{
    
}