using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.gRPC.Client.Services
{
    public class BasketGrpcClient : IBasketGrpcClient
    {
        private BasketGrpc.BasketGrpcClient client;
        private ILogger<BasketGrpcClient> logger;

        public BasketGrpcClient(BasketGrpc.BasketGrpcClient client, ILogger<BasketGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<GetUserBasketResponse> GetUserBasketAsync(Guid userId)
        {
            try
            {
                GetUserBasketRequest request = new GetUserBasketRequest
                {
                    UserId = userId.ToString()
                };

                logger.LogInformation("Requesting basket for user: {UserId}", userId);
                return await client.GetUserBasketAsync(request);
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error while getting basket for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<ClearUserBasketResponse> ClearUserBasketAsync(Guid userId)
        {
            try
            {
                ClearUserBasketRequest request = new ClearUserBasketRequest
                {
                    UserId = userId.ToString()
                };

                logger.LogInformation("Clearing basket for user: {UserId}", userId);
                return await client.ClearUserBasketAsync(request);
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error while clearing basket for user: {UserId}", userId);
                throw;
            }
        }
    }
}
