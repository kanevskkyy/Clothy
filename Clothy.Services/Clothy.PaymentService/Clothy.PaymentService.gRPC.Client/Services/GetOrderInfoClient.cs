using Clothy.PaymentService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.gRPC.Client.Services
{
    public class GetOrderInfoClient : IGetOrderInfoClient
    {
        private OrderServiceGrpc.OrderServiceGrpcClient grpcClient;
        private ILogger<GetOrderInfoClient> logger;

        public GetOrderInfoClient(OrderServiceGrpc.OrderServiceGrpcClient grpcClient, ILogger<GetOrderInfoClient> logger)
        {
            this.grpcClient = grpcClient;
            this.logger = logger;
        }

        public async Task<GetOrderInfoResponse> GetOrderInfoAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                GetOrderInfoRequest getOrderInfoRequest = new GetOrderInfoRequest
                {
                    OrderId = orderId.ToString()
                };
                logger.LogInformation("Requesting order info for order: {OrderId}", orderId);

                return await grpcClient.GetOrderInfoAsync(getOrderInfoRequest, cancellationToken: cancellationToken);
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error while getting order info for order: {OrderId}", orderId);
                throw;
            }
        }
    }
}
