using Clothy.CatalogService.DAL.UOW;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.gRPC.Server.Services
{
    public class ClotheStockServiceImpl : ClotheStockService.ClotheStockServiceBase
    {
        private IUnitOfWork unitOfWork;
        private ILogger<ClotheStockServiceImpl> logger;

        public ClotheStockServiceImpl(IUnitOfWork unitOfWork, ILogger<ClotheStockServiceImpl> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<GetTotalQuantityResponse> GetTotalQuantity(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching total clothes quantity...");
            try
            {
                int totalQuantity = await unitOfWork.ClothesStocks.GetTotalQuantityAsync(context.CancellationToken);
                logger.LogInformation("Successfully fetched total clothes quantity: {TotalQuantity}", totalQuantity);

                return new GetTotalQuantityResponse
                {
                    TotalQuantity = totalQuantity
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while fetching total clothes quantity...");
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during reading"));
            }
        }
    }
}
