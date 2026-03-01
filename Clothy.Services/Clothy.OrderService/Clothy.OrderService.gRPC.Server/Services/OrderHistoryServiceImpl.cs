using Clothy.OrderService.DAL.UOW;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Clothy.OrderService.gRPC.Server.Services;

public class OrderHistoryServiceImpl : OrderHistoryService.OrderHistoryServiceBase
{
    private IUnitOfWork unitOfWork;
    private ILogger<OrderHistoryServiceImpl> logger;

    public OrderHistoryServiceImpl(
        ILogger<OrderHistoryServiceImpl> logger,
        IUnitOfWork unitOfWork)
    {
        this.logger = logger;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<HasUserAlreadyOrderedResponse> HasUserAlreadyOrdered(HasUserAlreadyOrderedRequest request, ServerCallContext context)
    {
        logger.LogInformation("Checking if user {UserId} has already ordered", request.UserId);

        try
        {
            Guid userId = Guid.Parse(request.UserId);
            bool alreadyOrdered = await unitOfWork.Orders.HasUserAlreadyOrderedAsync(userId, context.CancellationToken);

            logger.LogInformation("User {UserId} has already ordered: {AlreadyOrdered}", request.UserId, alreadyOrdered);

            return new HasUserAlreadyOrderedResponse
            {
                HasOrdered = alreadyOrdered
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database error while checking order history for user {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during reading"));
        }
    }
}