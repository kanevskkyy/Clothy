using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Clients
{
    public class OrderGrpcClient : IOrderGrpcClient
    {
        private OrderStatsService.OrderStatsServiceClient client;
        private ILogger<OrderGrpcClient> logger;

        public OrderGrpcClient(OrderStatsService.OrderStatsServiceClient client, ILogger<OrderGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<OrderStats> GetOrderStatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching order stats...");
                OrderStats response = await client.GetOrderStatsAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cancellationToken);
                
                logger.LogInformation("Successfully fetched order stats: NewOrders: {NewOrdersCount}, TotalPrice: {TotalPrice}, PendingOrders: {PendingOrders}",response.NewOrdersCount, response.TotalPrice, response.PendingOrders);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching order stats");
                throw;
            }
        }
    }
}