using Clothy.BasketService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.BasketService.gRPC.Client.Services;

public class OrderHistoryGrpcClient : IOrderHistoryGrpcClient
{
    private OrderHistoryService.OrderHistoryServiceClient client;
    private ILogger<OrderHistoryGrpcClient> logger;

    public OrderHistoryGrpcClient(
        OrderHistoryService.OrderHistoryServiceClient client,
        ILogger<OrderHistoryGrpcClient> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    
    public async Task<bool> HasUserAlreadyOrderedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            HasUserAlreadyOrderedRequest request = new HasUserAlreadyOrderedRequest
            {
                UserId = userId.ToString(),
            };
            
            HasUserAlreadyOrderedResponse hasUserAlreadyOrderedResponse = await client.HasUserAlreadyOrderedAsync(request, cancellationToken: cancellationToken);
            return hasUserAlreadyOrderedResponse.HasOrdered;
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "gRPC error while checking order history for user {UserId}", userId);
            throw;
        }
    }
}