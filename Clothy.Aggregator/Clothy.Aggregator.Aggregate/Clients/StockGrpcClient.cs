using Clothy.Aggregator.Aggregate.Clients.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Aggregator.Aggregate.Clients
{
    public class StockGrpcClient : IStockGrpcClient
    {
        private ClotheStockService.ClotheStockServiceClient client;
        private ILogger<StockGrpcClient> logger;

        public StockGrpcClient(ClotheStockService.ClotheStockServiceClient client, ILogger<StockGrpcClient> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<GetTotalQuantityResponse> GetTotalQuantityAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Fetching total clothes quantity...");
                GetTotalQuantityResponse response = await client.GetTotalQuantityAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: cancellationToken);

                logger.LogInformation("Successfully fetched total clothes quantity: {TotalQuantity}", response.TotalQuantity);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching total clothes quantity");
                throw;
            }
        }
    }
}
