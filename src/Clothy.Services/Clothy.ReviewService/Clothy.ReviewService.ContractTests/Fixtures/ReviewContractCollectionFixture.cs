using Xunit;

namespace Clothy.ReviewService.ContractTests.Fixtures;

[CollectionDefinition("ReviewContract")]
public class ReviewContractCollectionFixture : ICollectionFixture<Infrastructure.ReviewGrpcWebFactory>
{
    
}