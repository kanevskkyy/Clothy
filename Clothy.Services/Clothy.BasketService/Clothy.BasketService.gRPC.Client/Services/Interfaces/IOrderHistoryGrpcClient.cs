namespace Clothy.BasketService.gRPC.Client.Services.Interfaces;

public interface IOrderHistoryGrpcClient
{
    Task<bool> HasUserAlreadyOrderedAsync(Guid userId, CancellationToken cancellationToken = default);
}