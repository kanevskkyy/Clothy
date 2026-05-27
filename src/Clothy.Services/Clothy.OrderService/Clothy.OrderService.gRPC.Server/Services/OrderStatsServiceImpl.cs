using Clothy.OrderService.DAL.UOW;
using DnsClient.Internal;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.OrderService.gRPC.Server.Services
{
    public class OrderStatsServiceImpl : OrderStatsService.OrderStatsServiceBase
    {
        public IUnitOfWork unitOfWork;
        public ILogger<OrderStatsServiceImpl> logger;

        public OrderStatsServiceImpl(IUnitOfWork unitOfWork, ILogger<OrderStatsServiceImpl> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<OrderStats> GetOrderStats(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            logger.LogInformation("Starting fetching order stats...");
            try
            {
                (int newOrdersCount, decimal totalPrice, int pendingOrdersCount) result = await unitOfWork.Orders.GetOrdersStatistics(context.CancellationToken);
                logger.LogInformation("Successfully fetched order stats!");

                return new OrderStats
                {
                    NewOrdersCount = result.newOrdersCount,
                    TotalPrice = result.totalPrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    PendingOrders = result.pendingOrdersCount
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while fetching order stats...");
                throw new RpcException(new Status(StatusCode.Internal, "Database error occurred during reading"));
            }
        }
    }
}
