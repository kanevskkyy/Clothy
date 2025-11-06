using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.gRPC.Client.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Clothy.OrderService.gRPC.Client.Services
{
    public class OrderItemValidatorGrpcClient : IOrderItemValidatorGrpcClient
    {
        private OrderItemValidator.OrderItemValidatorClient client;
        private ILogger<OrderItemValidatorGrpcClient> logger;

        public OrderItemValidatorGrpcClient(OrderItemValidator.OrderItemValidatorClient client, ILogger<OrderItemValidatorGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<ValidateOrderItemsResponse> ValidateOrderItemsAsync(List<OrderItemToValidate> items)
        {
            if (items == null || items.Count == 0) logger.LogWarning("Attempted to validate empty or null order items list");
            
            try
            {
                ValidateOrderItemsRequest request = new ValidateOrderItemsRequest();
                request.Items.AddRange(items);

                var response = await client.ValidateOrderItemsAsync(request);
                logger.LogInformation("Validated {Count} order items", items.Count);

                return response;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
            {
                logger.LogWarning(ex, "Invalid argument passed to OrderItemValidator gRPC service");
                throw;
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "gRPC error occurred while validating order items");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while validating order items");
                throw;
            }
        }
    }
}
